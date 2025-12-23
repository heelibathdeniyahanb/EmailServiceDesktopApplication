import { useState } from "react";
import Sidebar from "../components/Sidebar";
import Dashboard from "../components/Dashboard";
import Inbox from "./Inbox.jsx";
import Compose from "./Compose.jsx";

export default function DashboardPage() {
  const [page, setPage] = useState("dashboard");

  const renderPage = () => {
    if (page === "dashboard") return <Dashboard />;
    if (page === "inbox") return <Inbox />;
    if (page === "compose") return <Compose onBack={() => setPage("inbox")} />;
  };

  return (
    <div className="flex h-screen">
      <Sidebar navigate={setPage} />
      <div className="flex-1 bg-bg text-text overflow-y-auto">
        {renderPage()}
      </div>
    </div>
  );
}
