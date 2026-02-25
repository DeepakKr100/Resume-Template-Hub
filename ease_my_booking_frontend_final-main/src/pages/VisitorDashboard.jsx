import { useEffect, useState } from "react";
import { api } from "../services/api";
import BookingCard from "../components/BookingCard";
import { useAuth } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { loadRazorpay } from "../utils/loadRazorpay";
import ReviewModal from "../components/ReviewModal";
import { notify } from "../utils/toast";

export default function VisitorDashboard() {
  const [paidPurchases, setPaidPurchases] = useState([]);
  const [pendingPurchases, setPendingPurchases] = useState([]);
  const { logout } = useAuth();
  const navigate = useNavigate();
  const [reviewFor, setReviewFor] = useState({ open: false, placeId: null, placeName: "" });

  const load = async () => {
    try {
      const { data } = await api.get("/Bookings/my");

      const paid = [];
      const pending = [];
      for (const b of data) {
        if (b.paymentConfirmed) paid.push(b);
        else pending.push(b);
      }

      setPaidPurchases(paid);
      setPendingPurchases(pending);
    } catch (e) {
      if (e?.response?.status === 401) {
        logout();
        navigate("/login", { replace: true });
      } else {
        console.error(e);
      }
    }
  };

  useEffect(() => { load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const handlePayNow = async (booking) => {
    try {
      await loadRazorpay();

      const publicKey = (process.env.REACT_APP_RAZORPAY_KEY || "").trim();
      if (!publicKey) {
        notify.error("Razorpay key missing in .env");
        return;
      }

      const { data } = await api.post("/Bookings", {
        placeId: booking.placeId,
        visitDate: booking.visitDate,
        quantity: booking.quantity,
      });

      const { bookingId, orderId, amount, currency } = data;

      const rz = new window.Razorpay({
        key: publicKey,
        amount,
        currency,
        name: "Resume Template Market",
        description: `Order #${bookingId} - ${booking.place?.name ?? ""}`,
        order_id: orderId,
        handler: async (resp) => {
          await api.post("/Bookings/verifyPayment", {
            bookingId,
            orderId: resp.razorpay_order_id,
            paymentId: resp.razorpay_payment_id,
            signature: resp.razorpay_signature,
          });
          await load();
          notify.success("Payment successful! File access unlocked.", { duration: 3000 });
        },
      });

      rz.open();
    } catch (e) {
      if (e?.response?.status === 401) {
        logout();
        navigate("/login", { replace: true });
        return;
      }
      notify.error(e?.response?.data || "Payment init failed", { duration: 3000 });
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold mb-3">My Purchases</h2>

      <h3 className="text-xl font-semibold mb-2">Pending Payment</h3>
      <div className="grid gap-3 md:grid-cols-2">
        {pendingPurchases.length === 0 ? (
          <div className="opacity-70">No pending orders.</div>
        ) : (
          pendingPurchases.map((b) => (
            <BookingCard
              key={b.bookingId}
              booking={b}
              onPayNow={handlePayNow}
            />
          ))
        )}
      </div>

      <h3 className="text-xl font-semibold mt-6 mb-2">Purchased Templates</h3>
      <div className="grid gap-3 md:grid-cols-2">
        {paidPurchases.length === 0 ? (
          <div className="opacity-70">No purchased templates yet.</div>
        ) : (
          paidPurchases.map((b) => (
            <BookingCard
              key={b.bookingId}
              booking={b}
              onWriteReview={({ placeId, placeName }) =>
                setReviewFor({ open: true, placeId, placeName })
              }
            />
          ))
        )}
      </div>

      <ReviewModal
        open={reviewFor.open}
        onClose={() => setReviewFor({ open: false, placeId: null, placeName: "" })}
        placeId={reviewFor.placeId}
        placeName={reviewFor.placeName}
        onSuccess={load}
      />
    </div>
  );
}