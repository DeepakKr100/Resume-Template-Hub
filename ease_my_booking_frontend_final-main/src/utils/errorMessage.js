// src/utils/errorMessage.js
export function errorMessage(err, fallback = "Something went wrong.") {
  const data = err?.response?.data;

  // Plain string from API
  if (typeof data === "string") return data;

  // ASP.NET ProblemDetails object: { title, status, errors, traceId, ... }
  if (data && typeof data === "object") {
    if (typeof data.title === "string" && data.title.trim()) return data.title;

    // ModelState errors: { errors: { Field: ["msg1","msg2"] } }
    if (data.errors && typeof data.errors === "object") {
      const msgs = Object.values(data.errors)
        .flat()
        .filter(Boolean)
        .map(String);
      if (msgs.length) return msgs.join(" ");
    }

    if (typeof data.message === "string" && data.message.trim()) return data.message;

    try {
      return JSON.stringify(data);
    } catch {
      return fallback;
    }
  }

  // Axios / network / generic error
  if (typeof err?.message === "string" && err.message.trim()) return err.message;

  return fallback;
}