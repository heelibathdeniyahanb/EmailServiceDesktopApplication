import { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

// Icon components
const AttachmentIcon = () => (
  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
    <path d="M8 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM15 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z"></path>
    <path d="M3 4a1 1 0 00-1 1v10a1 1 0 001 1h1.05a2.5 2.5 0 014.9 0H10a1 1 0 001-1V5a1 1 0 00-1-1H3zM14 7a1 1 0 00-1 1v6.05A2.5 2.5 0 0115.95 10H17a1 1 0 001-1v-2a1 1 0 00-1-1h-3z"></path>
  </svg>
);

const GroupIcon = () => (
  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
    <path d="M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v2h8v-2zM16 15v2h2v-2zM2 8a2 2 0 11-4 0 2 2 0 014 0zM6 15v2H0v-2a4 4 0 016-3.87z"></path>
  </svg>
);

const getInitials = (email) => {
  const name = email?.split('@')[0] || 'U';
  return name.substring(0, 2).toUpperCase();
};

export default function Inbox({ onCompose }) {
  const navigate = useNavigate();
  const [emails, setEmails] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchEmails = async () => {
      try {
        setLoading(true);
        const response = await axios.get("http://localhost:5294/api/Email/user/1/inbox");
        setEmails(response.data);
        setError("");
      } catch (err) {
        setError("Failed to load emails");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchEmails();
  }, []);

  const handleCompose = () => {
    navigate('/compose');
  };

  const handleOnBack = () => {
    navigate('/dashboard');
  };

  return (
    <div className="h-screen bg-white">
      <div className="w-full bg-black border-r border-gray-300 p-4">
         <button
        onClick={handleOnBack}
        className="mb-4 text-muted hover:text-text w-64"
      >
        ← Back
      </button>
        <button
          onClick={handleCompose}
          className="w-full bg-white text-black py-2 rounded font-semibold hover:bg-gray-200 transition"
        >
          Compose
        </button>
      </div>

      <div className="flex-1 p-4 overflow-y-auto bg-white">
        {loading && <p className="text-gray-600">Loading emails...</p>}
        {error && <p className="text-red-500">{error}</p>}
        {emails.length === 0 && !loading && <p className="text-gray-600">No emails found</p>}
        {emails.map(e => (
          <div
            key={e.id}
            className="border-b border-gray-300 py-4 cursor-pointer hover:bg-gray-50 transition px-4 flex items-center gap-4"
          >
            {/* Avatar Circle */}
            <div className="w-12 h-12 rounded-full bg-black text-white flex items-center justify-center font-semibold flex-shrink-0">
              {getInitials(e.sender?.email)}
            </div>

            {/* Email Content */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <p className="font-semibold text-black">{e.sender?.fullName || e.sender?.email}</p>
                {/* Group Mail Icon */}
                {e.recipients && e.recipients.length > 1 && (
                  <div className="text-gray-600" title="Group Mail">
                    <GroupIcon />
                  </div>
                )}
              </div>
              <p className="font-medium text-black truncate">{e.subject}</p>
              <p className="text-sm text-gray-500 truncate">{e.body?.substring(0, 50)}</p>
            </div>

            {/* Attachment Icon */}
            {e.attachments && e.attachments.length > 0 && (
              <div className="text-gray-600 flex-shrink-0" title="Has Attachment">
                <AttachmentIcon />
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
