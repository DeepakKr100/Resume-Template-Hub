import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import LogoutButton from "./LogoutButton";
import { FileText } from "lucide-react";

export default function Navbar() {
  const { user } = useAuth();

  return (
    <div className="navbar bg-base-100 shadow-sm">
      <div className="container mx-auto px-4">
        <div className="flex-1">
          <Link to="/" className="text-xl font-bold flex items-center gap-2">
            <span className="text-primary">
              <FileText size={22} />
            </span>
            <span>ResumeTemplateHub</span>
          </Link>
        </div>

        <div className="flex items-center gap-3">
          <NavLink to="/templates" className="btn btn-ghost btn-sm">
            Templates
          </NavLink>

          {!user && (
            <>
              <NavLink to="/login" className="btn btn-sm">
                Login
              </NavLink>
              <NavLink to="/register" className="btn btn-outline btn-sm">
                Register
              </NavLink>
            </>
          )}

          {user?.role === "Visitor" && (
            <NavLink to="/dashboard/visitor" className="btn btn-ghost btn-sm">
              My Purchases
            </NavLink>
          )}

          {user?.role === "Owner" && (
            <NavLink to="/dashboard/owner" className="btn btn-ghost btn-sm">
              Seller Dashboard
            </NavLink>
          )}

          {user && <LogoutButton className="btn btn-error btn-sm text-white" />}
        </div>
      </div>
    </div>
  );
}