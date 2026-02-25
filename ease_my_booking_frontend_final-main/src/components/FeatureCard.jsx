export default function FeatureCard({ icon, title, desc }) {
  return (
    <div className="card bg-base-100 shadow-sm border border-base-200">
      <div className="card-body">
        <div className="flex items-start gap-3">
          <div className="text-2xl">{icon}</div>
          <div>
            <h3 className="font-semibold">{title}</h3>
            <p className="text-sm opacity-70 mt-1">{desc}</p>
          </div>
        </div>
      </div>
    </div>
  );
}