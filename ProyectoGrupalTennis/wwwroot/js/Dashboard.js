function mostrarVista(id) {
    document.querySelectorAll('#dashboard-view .vista').forEach(function (v) {
        v.classList.remove('active');
    });

    var target = document.getElementById(id);
    if (target) {
        target.classList.add('active');
    }

    window.scrollTo(0, 0);
}

window.addEventListener('load', function () {
    const ctxSesiones = document.getElementById('graficoSesiones');
    const ctxSeguimiento = document.getElementById('graficoSeguimiento');

    if (ctxSesiones && typeof Chart !== 'undefined') {
        new Chart(ctxSesiones, {
            type: 'doughnut',
            data: {
                labels: ['Mañana', 'Tarde', 'Noche'],
                datasets: [{
                    data: [8, 12, 5],
                    backgroundColor: ['#a5c422', '#c7db6d', '#e4efb2'],
                    borderWidth: 0
                }]
            },
            options: {
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                },
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }

    if (ctxSeguimiento && typeof Chart !== 'undefined') {
        new Chart(ctxSeguimiento, {
            type: 'line',
            data: {
                labels: ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4'],
                datasets: [{
                    data: [72, 78, 81, 88],
                    borderColor: '#a5c422',
                    backgroundColor: 'rgba(165,196,34,0.15)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                plugins: {
                    legend: {
                        display: false
                    }
                },
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }

});