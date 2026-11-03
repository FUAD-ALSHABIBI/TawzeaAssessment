fetch('./components/header.html').then(r => r.text()).then(html => {
    document.getElementById('Header').innerHTML = html;
  });

  fetch('./components/footer.html').then(r => r.text()).then(html => {
    document.getElementById('Footer').innerHTML = html;
    document.getElementById('Year').textContent = new Date().getFullYear();
  });