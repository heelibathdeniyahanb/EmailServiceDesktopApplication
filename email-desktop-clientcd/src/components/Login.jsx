import { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const Login = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
  e.preventDefault();
  setError("");
  setLoading(true);

  try {
    const response = await axios.post(
      "http://localhost:5294/api/User/login",
      { username, password }
    );

    const { token, user } = response.data;

    // TEMP storage (will upgrade)
    localStorage.setItem("token", token);
    localStorage.setItem("user", JSON.stringify(user));

    navigate("/inbox");
  } catch (err) {
    setError(err.response?.data?.message || "Login failed");
  } finally {
    setLoading(false);
  }
};


  return (
   <div className="min-h-screen flex items-center justify-center bg-black">

      <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md ">

        <h1 className="text-3xl font-bold mb-6 text-center text-black">Login</h1>
        {error && <p className="text-red-500 mb-4">{error}</p>}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-black font-medium mb-1">Email</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full border border-gray-300 rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-black"
              required
            />
          </div>
          <div>
            <label className="block text-black font-medium mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full border border-gray-300 rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-black"
              required
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-black text-white py-2 rounded-md hover:bg-gray-800 transition"
          >
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>
        <p className="mt-4 text-center text-gray-600">
          Don't have an account? <a href="/register" className="text-black underline">Register</a>
        </p>
      </div>
    </div>
  );
};

export default Login;
