import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import LogoutButton from "./LogoutButton";
import { FileText, Menu, X } from "lucide-react";
import { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";

export default function Navbar() {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const location = useLocation();

  // Close drawer on route change
  useEffect(() => {
    setOpen(false);
  }, [location.pathname]);

  const linkClass =
    "btn btn-ghost btn-sm w-full justify-start md:w-auto md:justify-center";

  return (
    <div className="navbar bg-base-100 shadow-sm sticky top-0 z-50">
      <div className="container mx-auto px-4 flex items-center justify-between">
        {/* Brand */}
        <Link to="/" className="text-lg sm:text-xl font-bold flex items-center gap-2">
          <span className="text-primary">
            <FileText size={22} />
          </span>
          <span className="truncate">ResumeTemplateHub</span>
        </Link>

        {/* Desktop nav links */}
        <div className="hidden md:flex items-center gap-3">
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

        {/* Mobile hamburger button */}
        <button
          className="btn btn-ghost btn-sm md:hidden"
          onClick={() => setOpen(!open)}
          aria-label="Toggle menu"
        >
          {open ? <X size={22} /> : <Menu size={22} />}
        </button>
      </div>

      {/* Mobile drawer */}
      {open && (
        <div className="md:hidden border-t border-base-200 bg-base-100 absolute top-full left-0 right-0 shadow-lg z-50">
          <div className="container mx-auto px-4 py-3 flex flex-col gap-1">
            <NavLink to="/templates" className={linkClass}>
              Templates
            </NavLink>

            {!user && (
              <>
                <NavLink to="/login" className={linkClass}>
                  Login
                </NavLink>
                <NavLink to="/register" className={linkClass}>
                  Register
                </NavLink>
              </>
            )}

            {user?.role === "Visitor" && (
              <NavLink to="/dashboard/visitor" className={linkClass}>
                My Purchases
              </NavLink>
            )}

            {user?.role === "Owner" && (
              <NavLink to="/dashboard/owner" className={linkClass}>
                Seller Dashboard
              </NavLink>
            )}

            {user && (
              <LogoutButton className="btn btn-error btn-sm text-white w-full mt-1" />
            )}
          </div>
        </div>
      )}
    </div>
  );
}