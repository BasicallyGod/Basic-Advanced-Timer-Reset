const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '..');
const skip = new Set(['.git', '.tools', 'bin', 'obj', 'node_modules', 'generated', 'artifacts']);
function walk(dir) {
  return fs.readdirSync(dir, {withFileTypes:true}).flatMap(entry => skip.has(entry.name) ? [] : entry.isDirectory() ? walk(path.join(dir,entry.name)) : [path.join(dir,entry.name)]);
}
const files = walk(root);
const spec = fs.readFileSync(path.join(root, 'docs/design/BAT-design-specification.md'), 'utf8');
const log = JSON.parse(fs.readFileSync(path.join(root, 'docs/design/bat-design-decisions.json'), 'utf8'));
assert(spec.includes('Version ' + log.specificationVersion), 'Specification and decision log version differ');
assert.equal(log.canonicalSpecification, 'docs/design/BAT-design-specification.md');
for (let i=1;i<=35;i++) assert(new RegExp('\\bR'+String(i).padStart(2,'0')+'\\b').test(spec), 'Missing requirement R'+i);
for (let i=1;i<=13;i++) assert(new RegExp('\\bO'+String(i).padStart(2,'0')+'\\b').test(spec), 'Missing open decision O'+i);
for (const file of files) {
  const relative = path.relative(root,file).split(path.sep).join('/');
  const content = fs.readFileSync(file, 'utf8');
  if (relative !== 'tools/validate.cjs') {
    assert(!/[A-Za-z]:[\\/]Users[\\/]/i.test(content), 'Workstation path in '+relative);
    assert(!/github_pat_[A-Za-z0-9_]{20,}|gh[pousr]_[A-Za-z0-9]{25,}|-----BEGIN (?:RSA |OPENSSH )?PRIVATE KEY-----/.test(content), 'Potential credential in '+relative);
  }
  if (!relative.endsWith('.md')) continue;
  for (const match of content.matchAll(/\]\(([^)]+)\)/g)) {
    const target = match[1].replace(/^<|>$/g,'').split('#')[0];
    if (!target || /^[a-z][a-z0-9+.-]*:/i.test(target)) continue;
    assert(fs.existsSync(path.resolve(path.dirname(file), decodeURIComponent(target))), 'Broken link in '+relative+': '+target);
  }
}
for (const licence of ['LICENSE-lucide.txt', 'LICENSE-toastui-editor.txt']) assert(fs.statSync(path.join(root, 'prototypes/vendor', licence)).size>100);
for (const filename of files.filter(file => /\.(cjs|js)$/.test(file) && !file.includes(path.sep+'vendor'+path.sep))) {
  // Source fragments may intentionally depend on their containing template.
  if (!filename.includes(path.sep+'src'+path.sep)) require('node:child_process').execFileSync(process.execPath, ['--check', filename]);
}
console.log(`PASS: ${files.length} source files; design versions, requirements, open decisions, local links, portable paths and vendor notices.`);
