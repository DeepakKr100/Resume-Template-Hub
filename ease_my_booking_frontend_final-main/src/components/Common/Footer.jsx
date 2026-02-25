import { Link } from "react-router-dom";

export default function Footer() {
  return (
    <footer className="mt-14 border-t border-base-300">
      <div className="container mx-auto px-4 py-10">
        <div className="grid gap-8 md:grid-cols-3">
          <div>
            <div className="text-lg font-bold">ResumeTemplateHub</div>
            <p className="text-sm opacity-70 mt-2">
              Marketplace for professional resume templates. Browse previews, pay securely,
              and access files instantly after purchase.
            </p>
          </div>

          <div>
            <div className="font-semibold mb-2">Explore</div>
            <div className="flex flex-col gap-2 text-sm">
              <Link className="link link-hover" to="/templates">Templates</Link>
              <Link className="link link-hover" to="/register">Become a Seller</Link>
              <Link className="link link-hover" to="/login">Login</Link>
            </div>
          </div>

          <div>
            <div className="font-semibold mb-2">Support</div>
            <div className="flex flex-col gap-2 text-sm opacity-80">
              <span>Secure payments (Razorpay)</span>
              <span>Instant access after purchase</span>
              <span>Preview images before buying</span>
            </div>
          </div>
        </div>

        <div className="mt-8 text-xs opacity-60">
          © {new Date().getFullYear()} ResumeTemplateHub. All rights reserved.
        </div>
      </div>
    </footer>
  );
}