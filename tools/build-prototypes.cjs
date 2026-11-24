const fs = require('node:fs');
const path = require('node:path');
require('./build-time-flow.cjs');
require('./build-notes-library.cjs');
require('./build-gadget.cjs');

const output = path.resolve(__dirname, '../prototypes/generated');
const titles = {
  'bat-day-dashboard': 'Working day',
  'bat-time-flow': 'Time entry',
  'bat-notes-library': 'Notes library',
  'bat-gadget': 'Connected pill and dashboard',
};
const links = Object.entries(titles).map(([name, title]) => `<a href="${name}.preview.html">${title}</a>`).join(' · ');
for (const [name, title] of Object.entries(titles)) {
  const fragment = fs.readFileSync(path.join(output, name + '.html'), 'utf8');
  // Small host adapter for the reference fragments; intentionally memory-only.
  const wrapper = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>BAT reference — ${title}</title>
<style>:root{color-scheme:light dark}body{margin:0;padding:16px;background:light-dark(#f7f7f3,#161e19);color:light-dark(#29362e,#e9eae6);font:14px/1.5 system-ui,sans-serif}body>nav{margin-bottom:16px}a{color:inherit}body>main{max-width:1280px;margin:auto}body>p{font-size:12px}</style>
<script src="../vendor/lucide-1.8.0.min.js"></script>
<script>window.openai={widgetState:null,setWidgetState:async function(value){this.widgetState=structuredClone(value)}};</script>
</head><body><nav aria-label="Prototype references">${links}</nav>
<main>${fragment}</main><p>Design experiment · fictional data · simulated clock · changes last until reload.</p></body></html>`;
  fs.writeFileSync(path.join(output, name + '.preview.html'), wrapper);
}
fs.writeFileSync(path.join(output, 'index.html'), '<!doctype html><html lang="en"><meta charset="utf-8"><title>BAT design references</title><h1>BAT design references</h1><nav>' + links + '</nav><p>Fictional data and simulated clocks. See the governing specification for accepted and proposed behaviour.</p></html>');
console.log('Built four standalone previews; serve with npm run preview.');
