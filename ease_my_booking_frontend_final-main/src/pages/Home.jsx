import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { api } from "../services/api";
import Footer from "../components/Common/Footer";
import FeatureCard from "../components/FeatureCard";

export default function Home() {
  const navigate = useNavigate();

  const quotes = useMemo(
    () => [
      "“Your resume is your first interview.”",
      "“Great opportunities start with great presentation.”",
      "“Stand out on paper before you stand out in person.”",
    ],
    []
  );

  const [templates, setTemplates] = useState([]);
  const [quoteIndex, setQuoteIndex] = useState(0);
  const [searchType, setSearchType] = useState("");
  const [maxPrice, setMaxPrice] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await api.get("/Places");
        const list = Array.isArray(res?.data) ? res.data : [];
        setTemplates(list.slice(0, 9));
      } catch (err) {
        console.error("Home: failed to load templates", err);
        setTemplates([]);
      }
    })();
  }, []);

  useEffect(() => {
    const timer = setInterval(() => {
      setQuoteIndex((prev) => (prev + 1) % quotes.length);
    }, 4500);
    return () => clearInterval(timer);
  }, [quotes.length]);

  const onSearch = () => {
    const params = new URLSearchParams();
    if (searchType.trim()) params.set("type", searchType.trim());
    if (maxPrice.trim()) params.set("max", maxPrice.trim());
    navigate(`/templates?${params.toString()}`);
  };

  const firstImageOf = (p) =>
    (p?.images && p.images.length > 0 && p.images[0]?.url) || p?.imageUrl || "";

  return (
    <div className="w-full">
      {/* HERO */}
      <div className="relative overflow-hidden">
        <div
          className="absolute inset-0"
          style={{
            backgroundImage:
              'url("https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?auto=format&fit=crop&w=1920&q=80")',
            backgroundSize: "cover",
            backgroundPosition: "center",
            filter: "saturate(1.1)",
          }}
        />
        <div className="absolute inset-0 bg-base-100/70 backdrop-blur-[2px]" />

        <div className="relative container mx-auto px-4 py-14 md:py-20">
          <div className="grid gap-10 md:grid-cols-2 items-center">
            <div>
              <AnimatePresence mode="wait">
                <motion.div
                  key={quoteIndex}
                  initial={{ opacity: 0, y: -8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: 8 }}
                  transition={{ duration: 0.35 }}
                  className="inline-block px-4 py-2 rounded-full bg-base-100 border border-base-200 shadow-sm text-sm opacity-80"
                >
                  {quotes[quoteIndex]}
                </motion.div>
              </AnimatePresence>

              <h1 className="text-4xl md:text-5xl font-extrabold mt-5 leading-tight">
                Buy & Sell <span className="text-primary">Professional</span> Resume Templates
              </h1>
              <p className="mt-4 text-base md:text-lg opacity-80 max-w-xl">
                Browse high-quality templates for MBA, consulting, product, finance and more.
                Preview images, pay securely, and access files instantly after purchase.
              </p>

              <div className="mt-6 flex gap-3 flex-wrap">
                <Link to="/templates" className="btn btn-primary">
                  Explore Templates
                </Link>
                <Link to="/register" className="btn btn-outline">
                  Become a Seller
                </Link>
              </div>

              {/* SEARCH BAR */}
              <div className="mt-8 card bg-base-100 shadow-sm border border-base-200">
                <div className="card-body">
                  <div className="font-semibold mb-2">Quick Search</div>
                  <div className="grid gap-3 md:grid-cols-3">
                    <input
                      className="input input-bordered w-full"
                      placeholder="Template type (MBA, PM, Consulting)"
                      value={searchType}
                      onChange={(e) => setSearchType(e.target.value)}
                    />
                    <input
                      className="input input-bordered w-full"
                      placeholder="Max price (e.g., 499)"
                      value={maxPrice}
                      onChange={(e) => setMaxPrice(e.target.value)}
                    />
                    <button className="btn btn-primary w-full" onClick={onSearch} type="button">
                      Search
                    </button>
                  </div>
                  <div className="text-xs opacity-60 mt-2">
                    Tip: Sellers add the Google Drive file link; buyers see it only after payment.
                  </div>
                </div>
              </div>
            </div>

            {/* HERO CARD */}
            <div className="md:justify-self-end w-full max-w-xl">
              <div className="card bg-base-100 shadow-xl border border-base-200">
                <div className="card-body">
                  <div className="flex items-center justify-between">
                    <div className="font-bold text-lg">Why ResumeTemplateHub?</div>
                    <span className="badge badge-primary badge-outline">Marketplace</span>
                  </div>

                  <div className="grid gap-3 mt-4">
                    <div className="flex items-start gap-3">
                      <div className="text-xl">🖼️</div>
                      <div>
                        <div className="font-semibold">Preview before you buy</div>
                        <div className="text-sm opacity-70">Multiple images help you choose confidently.</div>
                      </div>
                    </div>

                    <div className="flex items-start gap-3">
                      <div className="text-xl">🔒</div>
                      <div>
                        <div className="font-semibold">Secure payment</div>
                        <div className="text-sm opacity-70">Razorpay checkout with verification.</div>
                      </div>
                    </div>

                    <div className="flex items-start gap-3">
                      <div className="text-xl">⚡</div>
                      <div>
                        <div className="font-semibold">Instant file access</div>
                        <div className="text-sm opacity-70">Delivery link unlocks after payment.</div>
                      </div>
                    </div>
                  </div>

                  <div className="divider my-4" />

                  <div className="grid grid-cols-3 gap-3 text-center">
                    <div className="p-3 rounded-lg bg-base-200">
                      <div className="text-xl font-extrabold">{templates.length || 0}</div>
                      <div className="text-xs opacity-70">Featured</div>
                    </div>
                    <div className="p-3 rounded-lg bg-base-200">
                      <div className="text-xl font-extrabold">5★</div>
                      <div className="text-xs opacity-70">Reviews</div>
                    </div>
                    <div className="p-3 rounded-lg bg-base-200">
                      <div className="text-xl font-extrabold">INR</div>
                      <div className="text-xs opacity-70">Pricing</div>
                    </div>
                  </div>

                  <div className="mt-5">
                    <Link to="/templates" className="btn btn-primary w-full">
                      Browse All Templates
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* FEATURES */}
          <div className="mt-14 grid gap-4 md:grid-cols-3">
            <FeatureCard
              icon="📄"
              title="ATS-friendly designs"
              desc="Clean layouts that recruiters and ATS systems can parse easily."
            />
            <FeatureCard
              icon="🧩"
              title="Multiple categories"
              desc="MBA, consulting, product, finance, internships, and more."
            />
            <FeatureCard
              icon="🚀"
              title="Seller-ready"
              desc="Upload preview images + add Drive link for delivery after payment."
            />
          </div>

          {/* FEATURED TEMPLATES */}
          <div className="mt-14">
            <div className="flex items-center justify-between gap-4 flex-wrap">
              <h2 className="text-2xl font-bold">Featured Templates</h2>
              <Link to="/templates" className="btn btn-ghost">
                View all →
              </Link>
            </div>

            {templates.length === 0 ? (
              <div className="mt-4 opacity-70">
                No templates yet — add one from the Seller dashboard.
              </div>
            ) : (
              <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {templates.map((t) => (
                  <Link
                    key={t.placeId}
                    to={`/templates/${t.placeId}`}
                    className="card bg-base-100 shadow-sm border border-base-200 hover:shadow-md transition-shadow"
                  >
                    <figure className="aspect-[16/9] bg-base-200">
                      {firstImageOf(t) ? (
                        <img
                          src={firstImageOf(t)}
                          alt={t.name}
                          className="w-full h-full object-cover"
                          loading="lazy"
                        />
                      ) : (
                        <div className="w-full h-full grid place-items-center text-sm opacity-60">
                          No Image
                        </div>
                      )}
                    </figure>
                    <div className="card-body">
                      <h3 className="font-semibold line-clamp-1">{t.name}</h3>
                      <p className="text-sm opacity-70 line-clamp-2">{t.description}</p>
                      <div className="mt-2 flex items-center justify-between">
                        <span className="badge badge-outline">
                          Type: {t.templateType || t.location || "N/A"}
                        </span>
                        <span className="font-bold">₹{t.price}</span>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </div>

          {/* HOW IT WORKS */}
          <div className="mt-16 grid gap-6 md:grid-cols-3">
            <div className="card bg-base-100 border border-base-200 shadow-sm">
              <div className="card-body">
                <div className="text-sm opacity-60">Step 1</div>
                <div className="text-lg font-bold">Browse previews</div>
                <p className="text-sm opacity-70 mt-1">
                  Check multiple images and details before purchasing.
                </p>
              </div>
            </div>

            <div className="card bg-base-100 border border-base-200 shadow-sm">
              <div className="card-body">
                <div className="text-sm opacity-60">Step 2</div>
                <div className="text-lg font-bold">Pay securely</div>
                <p className="text-sm opacity-70 mt-1">
                  Razorpay checkout with payment verification.
                </p>
              </div>
            </div>

            <div className="card bg-base-100 border border-base-200 shadow-sm">
              <div className="card-body">
                <div className="text-sm opacity-60">Step 3</div>
                <div className="text-lg font-bold">Access the file</div>
                <p className="text-sm opacity-70 mt-1">
                  Delivery link appears in <b>My Purchases</b> after payment.
                </p>
              </div>
            </div>
          </div>

          {/* CTA */}
          <div className="mt-16 card bg-base-100 border border-base-200 shadow-sm">
            <div className="card-body md:flex md:items-center md:justify-between">
              <div>
                <div className="text-2xl font-bold">Ready to sell your templates?</div>
                <p className="opacity-70 mt-2">
                  Create a seller account, upload preview images, and add your Google Drive link for delivery.
                </p>
              </div>
              <div className="mt-4 md:mt-0 flex gap-3">
                <Link className="btn btn-primary" to="/register">
                  Become a Seller
                </Link>
                <Link className="btn btn-outline" to="/templates">
                  Explore Templates
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}