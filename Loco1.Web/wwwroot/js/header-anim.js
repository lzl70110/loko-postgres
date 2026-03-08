// Header locomotive animation (PNG frames + JS)
// - Pauses when tab is hidden (saves battery)
// - Uses the existing 12 frames from /icons/
(function () {
    const img = document.getElementById('locoHeaderAnim');
    if (!img) return;

    const base = '/icons/myicon-anim-';
    const frames = Array.from({ length: 12 }, (_, i) => `${base}${i + 1}.png`);
    // Preload frames
    frames.forEach(src => { const im = new Image(); im.src = src + '?preload=1'; });

    let idx = 0, timer = null, running = false;
    const fps = 7;                // ~7 FPS is smooth and light for header
    const interval = 1000 / fps;

    function tick() {
        img.src = frames[idx] + `?t=${idx}`;  // light cache-bust for mobile
        idx = (idx + 1) % frames.length;
    }

    function start() {
        if (running) return;
        running = true;
        tick();
        timer = setInterval(tick, interval);
    }

    function stop() {
        running = false;
        if (timer) { clearInterval(timer); timer = null; }
        img.src = frames[0] + '?t=0';
    }

    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') start(); else stop();
    });

    // Start immediately if page is visible; otherwise show first frame
    if (document.visibilityState === 'visible') start(); else stop();
})();