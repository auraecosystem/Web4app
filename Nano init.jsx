import React from 'react';
import { useWeb4 } from './Web4Context'; // Path to the provider above

export default function Web4KernelView() {
  const { app, user, data, aiInsight, events, setUser } = useWeb4();

  return (
    <div className="web4-container">
      <header>
        <h1>{app.name}</h1>
      </header>

      {/* 👤 USER CONTEXT LAYER */}
      <section className="context-layer">
        <h2>User</h2>
        <p>Name: {user.name}</p>
        <p>Status: <span className={`status-${user.status}`}>{user.status}</span></p>
        <button onClick={() => setUser(p => ({...p, status: p.status === 'active' ? 'inactive' : 'active'}))}>
          Toggle Status Simulation
        </button>
      </section>

      {/* 📊 DATA LAYER (API BOUND) */}
      <section className="data-layer">
        <h2>Live Data</h2>
        <p>Metric A: {data.metricA}</p>
        <p>Metric B: {data.metricB}</p>
      </section>

      {/* 🤖 AI LAYER */}
      <section className="ai-layer">
        <h2>AI Insight</h2>
        <p>{aiInsight}</p>
      </section>

      {/* ⚡ EVENT SYSTEM (Web4 CORE) */}
      <section className="event-system">
        <h2>Event Listeners ("update")</h2>
        {events.map((event, index) => (
          <div key={index} className="event-box animate-in">
            Update received: {event.message}
          </div>
        ))}
      </section>

      {/* 🔄 CONDITIONAL UI BLOCK */}
      <section className="conditional-ui">
        {user.status === "active" ? (
          <div className="unlocked-banner">
            <p>⚡ System fully unlocked</p>
          </div>
        ) : (
          <div className="locked-banner">
            <p>🔒 Limited access mode</p>
          </div>
        )}
      </section>
    </div>
  );
}
