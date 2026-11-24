const assert=require('node:assert/strict');
const {allocateSharedTime}=require('../prototypes/src/shared-time.js');
const e=(id,account,start,end)=>({id,account,start,end});
const near=(a,b)=>assert(Math.abs(a-b)<1e-9,`${a} differs from ${b}`);
let r=allocateSharedTime([e('a','Acme',600,660),e('b','Beacon',630,690)]);
assert.equal(r.total,90);assert.equal(r.shared,30);assert.equal(r.byAccount.get('Acme'),45);assert.equal(r.byAccount.get('Beacon'),45);
r=allocateSharedTime([e('a','Acme',0,60),e('b','Beacon',0,60),e('c','Cedar',0,60)]);
assert.equal(r.total,60);for(const value of r.byAccount.values())assert.equal(value,20);
r=allocateSharedTime([e('a','Acme',0,60),e('a2','Acme',0,60),e('b','Beacon',0,60)]);
assert.equal(r.byAccount.get('Acme'),30);assert.equal(r.byAccount.get('Beacon'),30);assert.equal(r.byBlock.get('a'),15);
r=allocateSharedTime([e('a','Acme',0,60),e('b','Beacon',10,20),e('c','Cedar',15,18),e('empty','Cedar',80,80),e('future','Beacon',100,90)]);
assert.equal(r.total,60);near([...r.byBlock.values()].reduce((a,b)=>a+b,0),r.total);near([...r.byAccount.values()].reduce((a,b)=>a+b,0),r.total);
r=allocateSharedTime([e('a','Acme',0,30),e('b','Beacon',30,60),e('c','Cedar',90,120)]);assert.equal(r.total,90);assert.equal(r.shared,0);
for(let count=1;count<30;count++){
  const entries=Array.from({length:count},(_,i)=>e('e'+i,'Account '+i%4,i%7,(i%7)+i+1));
  r=allocateSharedTime(entries);const reversed=allocateSharedTime([...entries].reverse());
  near(r.total,reversed.total);near([...r.byBlock.values()].reduce((a,b)=>a+b,0),r.total);
  for(const [id,value] of r.byBlock)near(value,reversed.byBlock.get(id));
}
console.log('PASS: union totals, equal account shares, repeated-account deduplication, triple/nested overlaps, empty/future exclusion, touching intervals and unrounded reconciliation.');
