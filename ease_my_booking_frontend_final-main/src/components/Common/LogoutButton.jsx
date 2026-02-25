import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { notify } from "../../utils/toast";

export default function LogoutButton({ className = "btn" }) {
  const { logout } = useAuth();
  const navigate = useNavigate();

  return (
    <button
      className={className}
      onClick={() => {
        logout();
        notify.success("Logged out successfully. See you soon!");
        navigate("/");
      }}
      type="button"
    >
      Logout
    </button>
  );
}