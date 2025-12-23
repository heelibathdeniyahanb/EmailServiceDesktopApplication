export default function Sidebar({ navigate }) {
  return (
    <div className="w-64 bg-card border-r border-border h-screen p-4">
      <h2 className="text-lg font-semibold mb-6">
        Company Mail
      </h2>

      <nav className="space-y-3">
        <NavItem label="Dashboard" onClick={() => navigate("dashboard")} />
        <NavItem label="Inbox" onClick={() => navigate("inbox")} />
        <NavItem label="Sent" onClick={() => navigate("sent")} />
        <NavItem label="Compose" onClick={() => navigate("compose")} />
      </nav>
    </div>
  );
}

function NavItem({ label, onClick }) {
  return (
    <div
      onClick={onClick}
      className="cursor-pointer px-3 py-2 rounded hover:bg-bg"
    >
      {label}
    </div>
  );
}
