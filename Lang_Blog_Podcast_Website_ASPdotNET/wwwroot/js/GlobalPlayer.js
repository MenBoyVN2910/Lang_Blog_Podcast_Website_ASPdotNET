/* ==========================================================================
   LẶNG. — GLOBAL PODCAST PLAYER (JavaScript Module)
   Quản lý phát nhạc toàn cục, lưu/khôi phục trạng thái qua sessionStorage.
   ========================================================================== */

const LangPlayer = (function () {
    'use strict';

    // ====== DOM Elements ======
    let playerEl, audioEl;
    let coverImg, trackTitle, trackAuthor;
    let btnPlay, btnSkipBack, btnSkipForward;
    let seekbarContainer, seekbarFill, timeCurrent, timeTotal;
    let volumeBtn, volumeContainer, volumeFill;
    let speedBtn, closeBtn;

    // ====== State ======
    let isInitialized = false;
    let isSeeking = false;
    let isVolumeAdjusting = false;
    let currentSpeed = 1;
    const speeds = [0.5, 0.75, 1, 1.25, 1.5, 2];
    let currentPodcastId = null;

    const STORAGE_KEY = 'lang_player_state';

    // ====== Helpers ======
    function formatTime(seconds) {
        if (isNaN(seconds) || !isFinite(seconds)) return '0:00';
        const m = Math.floor(seconds / 60);
        const s = Math.floor(seconds % 60);
        return m + ':' + (s < 10 ? '0' : '') + s;
    }

    function setAuthor(author) {
        if (!trackAuthor) return;
        var displayAuthor = (author && author.length > 15) ? author.substring(0, 15) + '...' : (author || '');
        trackAuthor.textContent = displayAuthor;
        trackAuthor.title = author || '';
    }

    function saveState() {
        if (!audioEl || !audioEl.src) return;
        const state = {
            src: audioEl.src,
            currentTime: audioEl.currentTime,
            duration: audioEl.duration || 0,
            volume: audioEl.volume,
            speed: currentSpeed,
            title: trackTitle ? trackTitle.textContent : '',
            author: trackAuthor ? (trackAuthor.getAttribute('title') || trackAuthor.textContent) : '',
            cover: coverImg ? coverImg.src : '',
            podcastId: currentPodcastId,
            paused: audioEl.paused,
            timestamp: Date.now()
        };
        try {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        } catch (e) { /* ignore */ }
    }

    function loadState() {
        try {
            const raw = sessionStorage.getItem(STORAGE_KEY);
            if (!raw) return null;
            return JSON.parse(raw);
        } catch (e) {
            return null;
        }
    }

    function clearState() {
        try {
            sessionStorage.removeItem(STORAGE_KEY);
        } catch (e) { /* ignore */ }
    }

    // ====== Initialization ======
    function init() {
        if (isInitialized) return;

        playerEl = document.getElementById('lang-global-player');
        if (!playerEl) return;

        audioEl = document.getElementById('gp-audio');
        coverImg = document.getElementById('gp-cover');
        trackTitle = document.getElementById('gp-title');
        trackAuthor = document.getElementById('gp-author');
        btnPlay = document.getElementById('gp-btn-play');
        btnSkipBack = document.getElementById('gp-btn-skip-back');
        btnSkipForward = document.getElementById('gp-btn-skip-forward');
        seekbarContainer = document.getElementById('gp-seekbar');
        seekbarFill = document.getElementById('gp-seekbar-fill');
        timeCurrent = document.getElementById('gp-time-current');
        timeTotal = document.getElementById('gp-time-total');
        volumeBtn = document.getElementById('gp-volume-btn');
        volumeContainer = document.getElementById('gp-volume-slider');
        volumeFill = document.getElementById('gp-volume-fill');
        speedBtn = document.getElementById('gp-speed-btn');
        closeBtn = document.getElementById('gp-btn-close');

        bindEvents();
        isInitialized = true;

        // Khôi phục trạng thái từ sessionStorage
        restoreState();
    }

    function bindEvents() {
        // Play/Pause
        btnPlay.addEventListener('click', togglePlayPause);

        // Skip ±15s
        btnSkipBack.addEventListener('click', function () {
            audioEl.currentTime = Math.max(0, audioEl.currentTime - 15);
        });
        btnSkipForward.addEventListener('click', function () {
            audioEl.currentTime = Math.min(audioEl.duration || 0, audioEl.currentTime + 15);
        });

        // Seekbar — Click
        seekbarContainer.addEventListener('click', function (e) {
            if (isSeeking) return;
            var rect = seekbarContainer.getBoundingClientRect();
            var pct = (e.clientX - rect.left) / rect.width;
            pct = Math.max(0, Math.min(1, pct));
            if (audioEl.duration) {
                audioEl.currentTime = pct * audioEl.duration;
            }
        });

        // Seekbar — Drag
        seekbarContainer.addEventListener('mousedown', startSeek);
        seekbarContainer.addEventListener('touchstart', startSeek, { passive: true });

        // Volume — Click
        volumeContainer.addEventListener('click', function (e) {
            var rect = volumeContainer.getBoundingClientRect();
            var pct = (e.clientX - rect.left) / rect.width;
            pct = Math.max(0, Math.min(1, pct));
            audioEl.volume = pct;
            updateVolumeUI();
            saveState();
        });

        // Volume — Drag
        volumeContainer.addEventListener('mousedown', startVolumeAdjust);

        // Volume — Mute toggle
        volumeBtn.addEventListener('click', function () {
            audioEl.muted = !audioEl.muted;
            updateVolumeIcon();
        });

        // Speed toggle
        speedBtn.addEventListener('click', cycleSpeed);

        // Close
        closeBtn.addEventListener('click', closePlayer);

        // Audio events
        audioEl.addEventListener('timeupdate', onTimeUpdate);
        audioEl.addEventListener('loadedmetadata', onMetadataLoaded);
        audioEl.addEventListener('ended', onEnded);

        // Save state periodically
        audioEl.addEventListener('pause', saveState);

        // Keyboard shortcuts
        document.addEventListener('keydown', onKeyDown);

        // Save state before unload
        window.addEventListener('beforeunload', saveState);
    }

    // ====== Seekbar Drag ======
    function startSeek(e) {
        isSeeking = true;
        document.addEventListener('mousemove', onSeekMove);
        document.addEventListener('mouseup', stopSeek);
        document.addEventListener('touchmove', onSeekMove, { passive: true });
        document.addEventListener('touchend', stopSeek);
    }

    function onSeekMove(e) {
        if (!isSeeking) return;
        var clientX = e.touches ? e.touches[0].clientX : e.clientX;
        var rect = seekbarContainer.getBoundingClientRect();
        var pct = (clientX - rect.left) / rect.width;
        pct = Math.max(0, Math.min(1, pct));
        seekbarFill.style.width = (pct * 100) + '%';
        timeCurrent.textContent = formatTime(pct * (audioEl.duration || 0));
    }

    function stopSeek(e) {
        if (!isSeeking) return;
        var clientX = e.changedTouches ? e.changedTouches[0].clientX : e.clientX;
        var rect = seekbarContainer.getBoundingClientRect();
        var pct = (clientX - rect.left) / rect.width;
        pct = Math.max(0, Math.min(1, pct));
        if (audioEl.duration) {
            audioEl.currentTime = pct * audioEl.duration;
        }
        isSeeking = false;
        document.removeEventListener('mousemove', onSeekMove);
        document.removeEventListener('mouseup', stopSeek);
        document.removeEventListener('touchmove', onSeekMove);
        document.removeEventListener('touchend', stopSeek);
    }

    // ====== Volume Drag ======
    function startVolumeAdjust(e) {
        isVolumeAdjusting = true;
        document.addEventListener('mousemove', onVolumeMove);
        document.addEventListener('mouseup', stopVolumeAdjust);
    }

    function onVolumeMove(e) {
        if (!isVolumeAdjusting) return;
        var rect = volumeContainer.getBoundingClientRect();
        var pct = (e.clientX - rect.left) / rect.width;
        pct = Math.max(0, Math.min(1, pct));
        audioEl.volume = pct;
        updateVolumeUI();
    }

    function stopVolumeAdjust() {
        isVolumeAdjusting = false;
        document.removeEventListener('mousemove', onVolumeMove);
        document.removeEventListener('mouseup', stopVolumeAdjust);
        saveState();
    }

    // ====== Playback Controls ======
    function togglePlayPause() {
        if (!audioEl.src) return;
        if (audioEl.paused) {
            audioEl.play().catch(function () { /* autoplay blocked */ });
        } else {
            audioEl.pause();
        }
        updatePlayIcon();
        saveState();
    }

    function cycleSpeed() {
        var idx = speeds.indexOf(currentSpeed);
        idx = (idx + 1) % speeds.length;
        currentSpeed = speeds[idx];
        audioEl.playbackRate = currentSpeed;
        speedBtn.textContent = currentSpeed + 'x';
        saveState();
    }

    // ====== UI Updates ======
    function updatePlayIcon() {
        if (audioEl.paused) {
            btnPlay.innerHTML = '<i class="fa-solid fa-play" style="transform: translateX(1px);"></i>';
        } else {
            btnPlay.innerHTML = '<i class="fa-solid fa-pause"></i>';
        }
    }

    function updateVolumeUI() {
        var vol = audioEl.muted ? 0 : audioEl.volume;
        volumeFill.style.width = (vol * 100) + '%';
        updateVolumeIcon();
    }

    function updateVolumeIcon() {
        var vol = audioEl.muted ? 0 : audioEl.volume;
        if (vol === 0) {
            volumeBtn.innerHTML = '<i class="fa-solid fa-volume-xmark"></i>';
        } else if (vol < 0.5) {
            volumeBtn.innerHTML = '<i class="fa-solid fa-volume-low"></i>';
        } else {
            volumeBtn.innerHTML = '<i class="fa-solid fa-volume-high"></i>';
        }
    }

    function onTimeUpdate() {
        if (isSeeking) return;
        if (audioEl.duration) {
            var pct = (audioEl.currentTime / audioEl.duration) * 100;
            seekbarFill.style.width = pct + '%';
        }
        timeCurrent.textContent = formatTime(audioEl.currentTime);

        // Lưu trạng thái mỗi 3 giây
        if (Math.floor(audioEl.currentTime) % 3 === 0) {
            saveState();
        }
    }

    function onMetadataLoaded() {
        timeTotal.textContent = formatTime(audioEl.duration);
        updatePlayIcon();
    }

    function onEnded() {
        updatePlayIcon();
        seekbarFill.style.width = '0%';
        timeCurrent.textContent = '0:00';
        saveState();
    }

    // ====== Keyboard ======
    function onKeyDown(e) {
        // Không xử lý phím tắt nếu đang focus vào input/textarea/select
        var tag = (e.target.tagName || '').toLowerCase();
        if (tag === 'input' || tag === 'textarea' || tag === 'select') return;

        // Không xử lý nếu player đang ẩn
        if (!playerEl || !playerEl.classList.contains('is-visible')) return;

        switch (e.code) {
            case 'Space':
                e.preventDefault();
                togglePlayPause();
                break;
            case 'ArrowLeft':
                e.preventDefault();
                audioEl.currentTime = Math.max(0, audioEl.currentTime - 15);
                break;
            case 'ArrowRight':
                e.preventDefault();
                audioEl.currentTime = Math.min(audioEl.duration || 0, audioEl.currentTime + 15);
                break;
        }
    }

    // ====== Show / Hide Player ======
    function showPlayer(slideUp) {
        if (!playerEl) return;
        if (slideUp) {
            playerEl.classList.remove('is-visible');
            playerEl.classList.add('is-sliding-up');
            // Sau animation, chuyển sang class cố định
            setTimeout(function () {
                playerEl.classList.remove('is-sliding-up');
                playerEl.classList.add('is-visible');
            }, 520);
        } else {
            playerEl.classList.remove('is-sliding-up');
            playerEl.classList.add('is-visible');
        }
        // Thêm padding-bottom cho body
        document.body.style.paddingBottom = (playerEl.offsetHeight + 20) + 'px';
    }

    function closePlayer() {
        if (!playerEl) return;
        audioEl.pause();
        audioEl.removeAttribute('src');
        audioEl.load();
        playerEl.classList.remove('is-visible', 'is-sliding-up');
        document.body.style.paddingBottom = '';
        clearState();
        currentPodcastId = null;
    }

    // ====== Restore State ======
    function restoreState() {
        var state = loadState();
        if (!state || !state.src) return;

        // Kiểm tra nếu state quá cũ (> 4 giờ)
        if (Date.now() - state.timestamp > 4 * 60 * 60 * 1000) {
            clearState();
            return;
        }

        // Khôi phục UI
        if (coverImg) coverImg.src = state.cover || '';
        if (trackTitle) trackTitle.textContent = state.title || '';
        setAuthor(state.author);
        currentPodcastId = state.podcastId || null;

        // Khôi phục speed
        currentSpeed = state.speed || 1;
        audioEl.playbackRate = currentSpeed;
        if (speedBtn) speedBtn.textContent = currentSpeed + 'x';

        // Khôi phục volume
        audioEl.volume = state.volume != null ? state.volume : 1;
        updateVolumeUI();
        
        // Khôi phục tiến trình ban đầu nếu có duration lưu lại
        if (state.duration) {
            var pct = (state.currentTime / state.duration) * 100;
            seekbarFill.style.width = pct + '%';
            timeCurrent.textContent = formatTime(state.currentTime);
            timeTotal.textContent = formatTime(state.duration);
        }

        // Cập nhật icon lúc mới restore
        if (state.paused) {
             btnPlay.innerHTML = '<i class="fa-solid fa-play" style="transform: translateX(1px);"></i>';
        } else {
             btnPlay.innerHTML = '<i class="fa-solid fa-pause"></i>';
        }

        // Khôi phục audio
        audioEl.src = state.src;

        audioEl.addEventListener('loadedmetadata', function onceLoaded() {
            audioEl.removeEventListener('loadedmetadata', onceLoaded);
            audioEl.currentTime = state.currentTime || 0;

            // Nếu trước đó đang phát thì tự động phát tiếp
            if (!state.paused) {
                audioEl.play().catch(function () {
                    // Autoplay bị chặn, chỉ cần cập nhật UI
                    updatePlayIcon();
                });
            } else {
                updatePlayIcon();
            }
        });

        // Hiển thị player ngay lập tức (tắt transition để không bị giật khi chuyển trang)
        playerEl.style.transition = 'none';
        showPlayer(false);
        // Buộc trình duyệt tính toán lại layout ngay lập tức (Force reflow)
        void playerEl.offsetHeight;
        playerEl.style.transition = '';
    }

    // ====== Public API ======
    function play(audioUrl, title, author, imageUrl, podcastId) {
        if (!isInitialized) init();
        if (!playerEl) return;

        // Nếu đang phát cùng bài thì toggle play/pause
        if (currentPodcastId && currentPodcastId === podcastId && audioEl.src) {
            togglePlayPause();
            return;
        }

        currentPodcastId = podcastId;

        // Cập nhật thông tin bài
        if (coverImg) {
            coverImg.src = imageUrl || '';
            coverImg.alt = title || '';
        }
        if (trackTitle) trackTitle.textContent = title || '';
        setAuthor(author);

        // Reset UI
        seekbarFill.style.width = '0%';
        timeCurrent.textContent = '0:00';
        timeTotal.textContent = '0:00';

        // Áp dụng speed
        audioEl.playbackRate = currentSpeed;

        // Load và phát
        audioEl.src = audioUrl;
        audioEl.load();
        audioEl.play().catch(function () {
            updatePlayIcon();
        });

        updatePlayIcon();
        showPlayer(true);
        saveState();
    }

    // Kiểm tra trạng thái cho trang ngoài
    function isPlaying(podcastId) {
        if (!audioEl || audioEl.paused) return false;
        if (podcastId && currentPodcastId !== podcastId) return false;
        return true;
    }

    function getCurrentPodcastId() {
        return currentPodcastId;
    }

    // ====== Auto-init khi DOM sẵn sàng ======
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return {
        play: play,
        togglePlayPause: togglePlayPause,
        isPlaying: isPlaying,
        getCurrentPodcastId: getCurrentPodcastId,
        close: closePlayer
    };
})();
