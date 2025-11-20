export function scrollToLyric(index){
  const container = document.getElementById('lyrics-scroll');
  if(!container) return;
  const el = document.getElementById(`lyric-${index}`);
  if(!el) return;
  const offsetTop = el.offsetTop - container.clientHeight/2 + el.clientHeight/2;
  container.scrollTo({ top: offsetTop, behavior:'smooth' });
}
