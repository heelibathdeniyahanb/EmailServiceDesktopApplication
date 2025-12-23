import { useState } from "react";
import api from "../api/apiClient";
import { useNavigate } from "react-router-dom";

export default function Compose({ onBack }) {
    const navigate = useNavigate();
  const [to, setTo] = useState("");
  const [subject, setSubject] = useState("");
  const [body, setBody] = useState("");

  const handleOnBack = () => {
    navigate('/inbox');
  };
  const sendEmail = async () => {
    await api.post("/Email", { to, subject, body });
    handleOnBack();
  };

  return (
    <div className="h-screen p-6">
      <button
        onClick={handleOnBack}
        className="mb-4 text-muted hover:text-text"
      >
        ← Back
      </button>

      <div className="max-w-3xl mx-auto bg-card p-6 rounded border border-border">
        <input
          className="w-full mb-3 p-2 bg-bg border border-border rounded"
          placeholder="To"
          onChange={e => setTo(e.target.value)}
        />

        <input
          className="w-full mb-3 p-2 bg-bg border border-border rounded"
          placeholder="Subject"
          onChange={e => setSubject(e.target.value)}
        />

        <textarea
          rows={10}
          className="w-full mb-4 p-2 bg-bg border border-border rounded"
          placeholder="Message"
          onChange={e => setBody(e.target.value)}
        />

        <button
          onClick={sendEmail}
          className="bg-text text-bg px-6 py-2 rounded"
        >
          Send
        </button>
      </div>
    </div>
  );
}
