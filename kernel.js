import React from 'react';
import { createRoot } from 'react-dom/client';
import { Web4Provider } from './Web4Context.js';
import Web4KernelView from './Web4KernelView.js';

// Activate Service Worker background threading for offline setups
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(err => console.error(err));
  });
}

// Select structural node and initialize virtual engine
const container = document.getElementById('root');
const root = createRoot(container);

root.render(
  <React.StrictMode>
    <Web4Provider initialConfig={{ appName: "Web4 React Kernel Ecosystem" }}>
      <Web4KernelView />
    </Web4Provider>
  </React.StrictMode>
);
