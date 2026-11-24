// Browser reference only. Production accounting remains behind the C# application boundary.
function allocateSharedTime(entries) {
  const valid=entries.filter(e=>Number.isFinite(e.start)&&Number.isFinite(e.end)&&e.end>e.start);
  const edges=[...new Set(valid.flatMap(e=>[e.start,e.end]))].sort((a,b)=>a-b);
  const byBlock=new Map(entries.map(e=>[e.id,0])),byAccount=new Map();
  let total=0,shared=0;
  for(let i=1;i<edges.length;i++){
    const start=edges[i-1],end=edges[i],active=valid.filter(e=>e.start<end&&e.end>start);
    if(!active.length)continue;
    const minutes=end-start,groups=new Map();
    for(const e of active){if(!groups.has(e.account))groups.set(e.account,[]);groups.get(e.account).push(e);}
    total+=minutes;if(active.length>1)shared+=minutes;
    for(const [account,items] of groups){
      const credit=minutes/groups.size;byAccount.set(account,(byAccount.get(account)||0)+credit);
      for(const e of items)byBlock.set(e.id,byBlock.get(e.id)+credit/items.length);
    }
  }
  return {total,shared,byBlock,byAccount};
}
if(typeof module!=='undefined')module.exports={allocateSharedTime};
