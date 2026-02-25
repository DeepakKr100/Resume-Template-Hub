import { toast } from "react-hot-toast";

function toText(input, fallback = "Something went wrong.") {
  if (input == null) return fallback;

  // plain string
  if (typeof input === "string") return input;

  // Handle raw ASP.NET ProblemDetails object directly
  if (input && typeof input === "object" && !input.response) {
    if (typeof input.title === "string" && input.title.trim()) {
      if (input.errors && typeof input.errors === "object") {
        const msgs = Object.values(input.errors).flat().filter(Boolean).map(String);
        if (msgs.length) return msgs.join(" ");
      }
      return input.title;
    }
    if (typeof input.message === "string" && input.message.trim()) return input.message;
  }

  // axios error object
  const data = input?.response?.data;

  if (typeof data === "string") return data;

  if (data && typeof data === "object") {
    if (typeof data.title === "string" && data.title.trim()) {
      if (data.errors && typeof data.errors === "object") {
        const msgs = Object.values(data.errors).flat().filter(Boolean).map(String);
        if (msgs.length) return msgs.join(" ");
      }
      return data.title;
    }

    if (typeof data.detail === "string" && data.detail.trim()) return data.detail;
    if (typeof data.message === "string" && data.message.trim()) return data.message;

    try {
      return JSON.stringify(data);
    } catch {
      return fallback;
    }
  }

  if (typeof input?.message === "string" && input.message.trim()) return input.message;

  try {
    return JSON.stringify(input);
  } catch {
    return fallback;
  }
}
export const notify = {
  success: (msg, opts) => toast.success(toText(msg, "Success"), opts),
  error: (msg, opts) => toast.error(toText(msg, "Something went wrong."), opts),
  info: (msg, opts) => toast(toText(msg, ""), opts),
  promise: (p, msgs, opts) =>
    toast.promise(
      p,
      {
        loading: toText(msgs?.loading, "Loading…"),
        success: (val) =>
          toText(typeof msgs?.success === "function" ? msgs.success(val) : msgs?.success, "Done."),
        error: (err) =>
          toText(typeof msgs?.error === "function" ? msgs.error(err) : msgs?.error ?? err, "Failed."),
      },
      opts
    ),
};