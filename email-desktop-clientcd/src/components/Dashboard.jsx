import { useEffect, useState } from "react";
import api from "../api/apiClient";

export default function Dashboard() {
  const [stats, setStats] = useState({
    total: 0,
    unread: 0,
    groups: 0
  });

  useEffect(() => {
    api.get("/emails/dashboard").then(res => setStats(res.data));
  }, []);

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold mb-6">Dashboard</h1>

      <div className="grid grid-cols-3 gap-6">
        <Card title="Total Emails" value={stats.total} />
        <Card title="Unread Emails" value={stats.unread} />
        <Card title="Group Emails" value={stats.groups} />
      </div>
    </div>
  );
}

function Card({ title, value }) {
  return (
    <div className="bg-card border border-border rounded p-6">
      <p className="text-muted text-sm">{title}</p>
      <p className="text-3xl font-bold mt-2">{value}</p>
    </div>
  );
}
