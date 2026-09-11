// Offline checks against the actual prototype script using a small DOM test double.
// This deliberately does not launch a browser or claim to verify browser layout.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = process.argv[2];
if (!path) throw new Error('Pass the absolute prototype fragment path.');
const html = fs.readFileSync(path, 'utf8');
const script = html.match(/<script>([\s\S]*?)<\/script>/)[1];
const source = html.replace(/<(script|style)>[\s\S]*?<\/\1>/g, '');
const checks = [];
function check(name, action) { action(); checks.push(name); }
class Element {
  constructor(tag, attrs = {}) {
    this.tag = tag; this.attrs = attrs; this.children = []; this.parent = null;
    this.events = {}; this.value = attrs.value || ''; this._text = ''; this._html = '';
    this.hidden = 'hidden' in attrs; this.disabled = 'disabled' in attrs;
    this.dataset = {};
    for (const [key, value] of Object.entries(attrs)) if (key.startsWith('data-')) this.dataset[key.slice(5).replace(/-([a-z])/g, (_, c) => c.toUpperCase())] = value;
    this.style = { setProperty(key, value) { this[key] = value; } };
    this.classList = { toggle: (name, on) => {
      const classes = new Set((this.attrs.class || '').split(' ').filter(Boolean));
      if (on) classes.add(name); else classes.delete(name); this.attrs.class = [...classes].join(' ');
    } };
    this.selectionStart = this.selectionEnd = 0; this.isConnected = true;
  }
  get type() { return this.attrs.type || ''; }
  set textContent(text) { this._text = String(text); }
  get textContent() { return this._text + this.children.map(x => x.textContent).join(''); }
  set innerHTML(text) { this._html = text; }
  get innerHTML() { return this._html; }
  append(child) { child.parent = this; this.children.push(child); }
  replaceChildren() { this.children = []; }
  matches(selector) {
    if (selector[0] === '#') return this.attrs.id === selector.slice(1);
    if (selector[0] === '.') return (this.attrs.class || '').split(' ').includes(selector.slice(1));
    const match = selector.match(/^\[([^=\]]+)(?:="([^"]*)")?\]$/);
    if (match) return match[1] in this.attrs && (match[2] === undefined || this.attrs[match[1]] === match[2]);
    return this.tag === selector;
  }
  querySelectorAll(selector) {
    const tokens = selector.split(' '); const last = tokens.pop(); const matches = [];
    const walk = element => {
      for (const child of element.children) {
        if (child.matches(last)) {
          let parent = child.parent, i = tokens.length - 1;
          while (parent && i >= 0) { if (parent.matches(tokens[i])) i--; parent = parent.parent; }
          if (i < 0) matches.push(child);
        }
        walk(child);
      }
    }; walk(this); return matches;
  }
  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
  setAttribute(key, value) { this.attrs[key] = String(value); }
  addEventListener(name, handler) { (this.events[name] ||= []).push(handler); }
  emit(name, event = {}) { for (const handler of this.events[name] || []) handler(event); }
  click() { if (!this.disabled) this.emit('click'); }
  focus() { document.activeElement = this; }
  scrollIntoView() { this.scrolled = true; }
  setRangeText(text, start, end) { this.value = this.value.slice(0, start) + text + this.value.slice(end); this.selectionStart = start; this.selectionEnd = start + text.length; }
}
const wrapper = new Element('wrapper'); const stack = [wrapper];
for (const token of source.matchAll(/<\/?[^>]+>|[^<]+/g)) {
  const text = token[0];
  if (text.startsWith('</')) { stack.pop(); continue; }
  if (text.startsWith('<')) {
    const tag = text.match(/^<([\w-]+)/)?.[1]; if (!tag) continue;
    const attrs = {};
    for (const m of text.slice(tag.length + 1, -1).matchAll(/([\w-]+)(?:="([^"]*)")?/g)) attrs[m[1]] = m[2] || '';
    const element = new Element(tag, attrs); stack.at(-1).append(element);
    if (!['input', 'br', 'hr', 'img', 'meta', 'link'].includes(tag)) stack.push(element);
  } else stack.at(-1)._text += text;
}
const root = wrapper.querySelector('#mdplayer-design');
const document = { getElementById: id => wrapper.querySelector('#' + id), createElement: tag => new Element(tag), activeElement: null };
const tweakers = [];
class Tweak {
  constructor(options) { this.options = options; tweakers.push(this); }
  addSelect(state) { this.state = state; }
  addToggle(state) { this.state = state; }
}
const timers = [];
const context = { document, Tweak, setTimeout(fn) { timers.push(fn); return timers.length; }, clearTimeout(id) { if (id) timers[id - 1] = null; }, console };
vm.runInNewContext(script, context, { timeout: 2000 });
const q = selector => { const element = root.querySelector(selector); assert(element, 'Missing selector ' + selector); return element; };
const click = action => q('[data-action="' + action + '"]').click();
const mode = mode => q('[data-mode-button="' + mode + '"]').click();
const dirty = () => !q('[data-action="save"]').hidden;
const edit = text => { q('textarea').value = text; q('textarea').emit('input'); for (const fn of timers.splice(0)) if (fn) fn(); };
const pref = (key, value) => { const element = q('[data-pref="' + key + '"]'); element.value = value; element.emit('input'); };
const rendered = () => q('[data-rendered]').innerHTML;
const initial = q('textarea').value;
check('Initial rendered Read mode; editor and Save hidden', () => {
  assert.equal(root.dataset.mode, 'read'); assert(q('.md-editor').hidden); assert(!dirty()); assert(rendered().includes('<h1'));
});
check('Typography updates appearance without changing source or dirty state', () => {
  click('type'); assert(!q('.md-type').hidden);
  pref('size', '24'); pref('leading', '1.9'); pref('gap', '1.2'); pref('width', '80'); pref('font', 'sans'); pref('align', 'justify');
  assert.equal(root.style['--md-size'], '24px'); assert.equal(root.style['--md-gap'], '1.2em'); assert.equal(root.style['--md-align'], 'justify');
  assert.equal(q('textarea').value, initial); assert(!dirty());
});
check('Closing typography returns focus to its trigger', () => { click('close-type'); assert(q('.md-type').hidden); assert.equal(document.activeElement, q('[data-action="type"]')); });
check('Entering and leaving Edit without typing stays clean', () => { mode('edit'); assert(!q('.md-editor').hidden); assert(q('.md-doc').hidden); mode('read'); assert(!dirty()); });
check('Edit then Read previews unsaved buffer without saving', () => {
  mode('edit'); edit('# Changed title\n\nA **deliberate** edit.'); mode('read'); assert(dirty()); assert(rendered().includes('Changed title')); assert(rendered().includes('<strong>deliberate</strong>'));
});
check('Split exposes both source and preview', () => { mode('split'); assert(!q('.md-doc').hidden); assert(!q('.md-editor').hidden); });
check('Cancel close preserves dirty source and mode', () => { click('close'); assert(!q('.md-overlay').hidden); assert.equal(document.activeElement, q('[data-action="cancel"]')); click('cancel'); assert(q('.md-overlay').hidden); assert(dirty()); assert.equal(root.dataset.mode, 'split'); });
check('Explicit save marks only the preview copy clean and explains no disk write', () => { click('save'); assert(!dirty()); assert(q('[data-message]').textContent.includes('No file was written')); });
const saved = q('textarea').value;
check('Discard on replacement returns to last saved sample in Read', () => { edit('# Discard me'); click('open'); assert(!q('.md-overlay').hidden); click('discard'); assert.equal(q('textarea').value, saved); assert(!dirty()); assert.equal(root.dataset.mode, 'read'); });
check('Save-and-close reaches empty state, reopening preserves saved sample and enters Read', () => { mode('edit'); edit('# Saved on close'); click('close'); click('save-continue'); assert(!q('.md-empty').hidden); click('open-empty'); assert(q('.md-empty').hidden); assert.equal(root.dataset.mode, 'read'); assert(rendered().includes('Saved on close')); });
check('Reverting exact source clears dirty state', () => { mode('edit'); const clean = q('textarea').value; edit(clean + '!'); assert(dirty()); edit(clean); assert(!dirty()); });
check('Formatting updates source and dirty state', () => { q('textarea').selectionStart = 0; q('textarea').selectionEnd = 1; q('[data-format="bold"]').click(); assert(dirty()); assert(q('textarea').value.startsWith('**#**')); });
check('Preview escapes raw HTML', () => { edit('# Literal markup\n\n<script>alert(1)</script>\n\n<img src=x onerror=alert(1)>'); assert(!rendered().includes('<script>')); assert(!rendered().includes('<img')); assert(rendered().includes('&lt;script&gt;')); });
check('All three design directions retain the source buffer', () => {
  const before = q('textarea').value;
  for (const direction of ['slate', 'studio', 'paper']) { tweakers[0].state.direction = direction; tweakers[0].options.onChange(); assert.equal(root.dataset.direction, direction); assert.equal(q('textarea').value, before); }
});
check('System, light and dark appearance retain dirty state', () => { for (const appearance of ['system', 'light', 'dark']) { pref('appearance', appearance); assert.equal(root.style.colorScheme, appearance === 'system' ? 'light dark' : appearance); assert(dirty()); } });
check('Focus mode has an explicit exit and preserves unsaved edits', () => { click('focus'); assert(q('.md-outline').hidden); assert.equal(q('[data-action="focus"]').textContent, 'Exit focus'); click('focus'); assert(dirty()); });
check('No network calls or document persistence in prototype', () => { assert(!/\b(fetch|XMLHttpRequest|WebSocket|localStorage|sessionStorage|indexedDB)\b/.test(script)); });
check('Fragment structure and JavaScript syntax', () => { assert(html.startsWith('<div')); assert(!/<(?:html|body|head|!doctype)\b/i.test(html)); assert(Buffer.byteLength(html) < 1000000); new vm.Script(script); });
// Verify actual opaque text/surface token pairs, independently of the DOM test double.
const palettes = [...html.matchAll(/--md-panel:light-dark\((#[\da-f]+),(#[\da-f]+)\);[^}]+/g)].map(m => Object.fromEntries([...m[0].matchAll(/--md-([\w-]+):light-dark\((#[\da-f]+),(#[\da-f]+)\)/g)].map(x => [x[1], [x[2], x[3]]])));
function luminance(hex) { const rgb = hex.slice(1).match(/../g).map(n => parseInt(n,16) / 255).map(n => n <= .04045 ? n / 12.92 : ((n + .055) / 1.055) ** 2.4); return rgb[0]*.2126+rgb[1]*.7152+rgb[2]*.0722; }
function contrast(a,b) { const x=luminance(a),y=luminance(b);return (Math.max(x,y)+.05)/(Math.min(x,y)+.05); }
const contrasts=[];
check('Text contrast >= 4.5:1 across three directions and both themes', () => {
  assert.equal(palettes.length, 3);
  palettes.forEach((palette,i) => [0,1].forEach(theme => {
    for(const [foreground,background] of [['ink','page'],['muted','panel'],['accent','selected'],['button-text','accent'],['muted','page']]) {
      const ratio=contrast(palette[foreground][theme],palette[background][theme]);
      contrasts.push({direction:['paper','slate','studio'][i],theme:['light','dark'][theme],pair:foreground+'/'+background,ratio:Number(ratio.toFixed(2))});
      assert(ratio>=4.5, `${i}/${theme} ${foreground}/${background}: ${ratio}`);
    }
  }));
});
console.log(JSON.stringify({checksPassed:checks.length,checks,contrastPairs:contrasts,limitations:['DOM test double; no browser layout, font shaping, focus behavior, or assistive-technology verification.','No Avalonia build, native file I/O, save-failure tests, or performance measurements.']},null,2));
