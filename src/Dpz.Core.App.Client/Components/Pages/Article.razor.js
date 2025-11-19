// JS Isolation for Article page
let _dotnetRef; let startY=0; let isPulling=false; const threshold=70; const body=document.body; let _observer;

export function init(dotnetRef){
    _dotnetRef = dotnetRef;
    setupPullToRefresh();
    observeSentinel();
}

export function reobserveSentinel(){
    // 重新观察（标签切换或搜索后列表被替换）
    if(_observer){ _observer.disconnect(); }
    observeSentinel();
}

export function dispose(){
    teardownPullToRefresh();
    if(_observer){ _observer.disconnect(); _observer = null; }
    _dotnetRef = null;
}

function setupPullToRefresh(){
    body.addEventListener('touchstart', onTouchStart,{passive:true});
    body.addEventListener('touchmove', onTouchMove,{passive:true});
    body.addEventListener('touchend', onTouchEnd,{passive:true});
}
function teardownPullToRefresh(){
    body.removeEventListener('touchstart', onTouchStart);
    body.removeEventListener('touchmove', onTouchMove);
    body.removeEventListener('touchend', onTouchEnd);
}
function onTouchStart(e){ if(window.scrollY===0){ startY = e.touches[0].clientY; isPulling=true; } }
function onTouchMove(e){ if(!isPulling) return; const diff=e.touches[0].clientY - startY; if(diff>threshold){ triggerRefresh(); isPulling=false; } }
function onTouchEnd(){ isPulling=false; }
function triggerRefresh(){ if(_dotnetRef){ _dotnetRef.invokeMethodAsync('OnPullToRefresh'); } }

function observeSentinel(){
    const sentinel = document.getElementById('infinite-scroll-sentinel');
    if(!sentinel || !_dotnetRef) return;
    _observer = new IntersectionObserver(entries=>{
        entries.forEach(entry=>{ if(entry.isIntersecting){ _dotnetRef.invokeMethodAsync('LoadNextPageAsyncJs').catch(()=>{}); } });
    },{ root:null, threshold:0.1 });
    _observer.observe(sentinel);
}
