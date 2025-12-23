import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Login from "./components/Login.jsx";
import Inbox from "./Pages/Inbox.jsx";
import Compose from "./Pages/Compose.jsx";
import DashboardPage from "./Pages/DashboardPage.jsx";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/inbox" element={<Inbox />} />
        <Route path="/compose" element={<Compose />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        {/* Add other routes */}
      </Routes>
    </Router>
  );
}

export default App;
