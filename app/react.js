import React, { createContext, useContext, useState, useEffect } from 'react';

// Initialize Web4 Context for Global State
const Web4Context = createContext(null);

export function Web4Provider({ children, initialConfig }) {
  // Layer States
  const [app, setApp] = useState({ name: initialConfig?.appName || "Web4 Cloud App" });
  const [user, setUser] = useState({ name: "User", status: "inactive" });
  const [data, setData] = useState({ metricA: 0, metricB: "Connecting..." });
  const [aiInsight, setAiInsight] = useState("Analyzing core conditions...");
  const [events, setEvents] = useState([]);

  // Mock data layer streaming & AI prompt generation
  useEffect(() => {
    // 1. Live Data Sync simulation
    const dataTimer = setInterval(() => {
      setData(prev => ({
        metricA: Math.floor(Math.random() * 100),
        metricB: prev.metricA > 50 ? "Optimal Network State" : "Standard Sync"
      }));
    }, 4000);

    // 2. Real-time Event System simulation
    const eventTimer = setInterval(() => {
      setEvents(prev => [...prev, { message: `Telemetry updated at ${new Date().toLocaleTimeString()}` }]);
    }, 7000);

    return () => {
      clearInterval(dataTimer);
      clearInterval(eventTimer);
    };
  }, []);

  // AI inference trigger tied to UI state updates
  useEffect(() => {
    if (data.metricA > 0) {
      setAiInsight(`AI Context Engine says: Metric A is shifting to ${data.metricA}. Resource thresholds are stable.`);
    }
  }, [data.metricA]);

  return (
    <Web4Context.Provider value={{ app, user, setUser, data, aiInsight, events }}>
      {children}
    </Web4Context.Provider>
  );
}

export const useWeb4 = () => useContext(Web4Context);
