
// Animate favicon by cycling through PNG frames (mobile-friendly)
(function () {
  const frames = [
    '/icons/myicon-anim-1.png',
    '/icons/myicon-anim-2.png',
    '/icons/myicon-anim-3.png',
    '/icons/myicon-anim-4.png',
    '/icons/myicon-anim-5.png',
    '/icons/myicon-anim-6.png',
    '/icons/myicon-anim-7.png',
    '/icons/myicon-anim-8.png',
    '/icons/myicon-anim-9.png',
    '/icons/myicon-anim-10.png',
    '/icons/myicon-anim-11.png',
    '/icons/myicon-anim-12.png'
  ];
  const frameDurationMs = 140; // ~7 FPS

  function getIconLink() {
    let link = document.querySelector("link[rel='icon']");
    if (!link) {
      link = document.createElement('link');
      link.setAttribute('rel', 'icon');
      link.setAttribute('type', 'image/png');
      document.head.appendChild(link);
    }
    return link;
  }

  // Preload frames
  frames.forEach(src => { const i = new Image(); i.src = src + '?preload=' + Date.now(); });

  let idx = 0; let timer = null; let running = false;

  function setFavicon(src) {
    const link = getIconLink();
    link.href = src + '?t=' + idx; // cache-bust for mobile
  }

  function tick() { setFavicon(frames[idx]); idx = (idx + 1) % frames.length; }

  function start() { if (running) return; running = true; tick(); timer = setInterval(tick, frameDurationMs); }
  function stop()  { running = false; if (timer) { clearInterval(timer); timer = null; } setFavicon(frames[0]); }

  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') start(); else stop();
  });

  window.addEventListener('focus', () => { if (!running && document.visibilityState === 'visible') start(); });

  if (document.visibilityState === 'visible') start(); else stop();
})();
