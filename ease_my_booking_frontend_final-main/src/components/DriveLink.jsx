// src/components/DriveLink.jsx
export default function DriveLink({
  url,
  label = "Open File",
  variant = "primary", // "primary" | "outline" | "ghost"
  size = "sm",         // "sm" | "md" | "lg"
  className = "",
}) {
  if (!url) return null;

  const variantClass =
    variant === "outline" ? "btn-outline"
    : variant === "ghost"  ? "btn-ghost"
    : "btn-primary";

  const sizeClass =
    size === "lg" ? "btn-lg"
    : size === "md" ? "btn-md"
    : "btn-sm";

  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      aria-label={label}
      title={label}
      className={[
        "group btn no-underline gap-2 rounded-full",
        variantClass,
        sizeClass,
        "shadow-md hover:shadow-lg active:scale-[.98] transition-all",
        className,
      ].join(" ")}
    >
      <FileIcon className="w-4 h-4 md:w-5 md:h-5" />
      <span>{label}</span>
      <ExternalIcon className="w-3.5 h-3.5 md:w-4 md:h-4 opacity-80" />
    </a>
  );
}

function FileIcon({ className = "" }) {
  return (
    <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true" className={className}>
      <path d="M14 2H7a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8l-5-6Zm0 2.5L17.5 9H14V4.5ZM8 13h8v1.5H8V13Zm0 3h8v1.5H8V16Zm0-6h4v1.5H8V10Z"/>
    </svg>
  );
}

function ExternalIcon({ className = "" }) {
  return (
    <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true" className={className}>
      <path d="M14 3h7v7h-2V7.414l-9.293 9.293-1.414-1.414L17.586 6H14V3z" />
      <path d="M5 5h6v2H7v10h10v-4h2v6H5z" />
    </svg>
  );
}