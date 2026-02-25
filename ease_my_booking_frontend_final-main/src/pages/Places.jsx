import { useEffect, useState } from "react";
import { api } from "../services/api";
import PlaceCard from "../components/PlaceCard";
import { notify } from "../utils/toast";
import { useSearchParams } from "react-router-dom";
import { errorMessage } from "../utils/errorMessage";

export default function Places() {
  const [places, setPlaces] = useState([]);
  const [q, setQ] = useState("");
  const [max, setMax] = useState("");
  const [loading, setLoading] = useState(false);
  const [searchParams] = useSearchParams();

  useEffect(() => {
    const typeFromUrl = searchParams.get("type") || "";
    const maxFromUrl = searchParams.get("max") || "";
    setQ(typeFromUrl);
    setMax(maxFromUrl);
    load(typeFromUrl, maxFromUrl);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function load(type = q, maxPrice = max) {
    const params = {};
    if (type) params.templateType = type;
    if (maxPrice) params.maxPrice = maxPrice;

    setLoading(true);
    try {
      const res = await api.get("/Places", { params });
      setPlaces(Array.isArray(res?.data) ? res.data : []);
    } catch (err) {
      console.error("Failed to load templates:", err);
      setPlaces([]);
      notify.error(errorMessage(err, "Unable to load templates right now."), { duration: 3000 });
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex gap-2 flex-wrap">
        <input
          className="input input-bordered"
          placeholder="Template type (e.g., MBA, PM, Consulting)"
          value={q}
          onChange={(e) => setQ(e.target.value)}
        />
        <input
          className="input input-bordered"
          placeholder="Max price"
          value={max}
          onChange={(e) => setMax(e.target.value)}
        />
        <button className="btn btn-primary" onClick={() => load(q, max)} type="button">
          Search
        </button>
      </div>

      {loading ? (
        <div className="opacity-70">Loading templates…</div>
      ) : places.length === 0 ? (
        <div className="opacity-70">No templates found.</div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {places.map((p) => (
            <PlaceCard key={p.placeId} place={p} />
          ))}
        </div>
      )}
    </div>
  );
}