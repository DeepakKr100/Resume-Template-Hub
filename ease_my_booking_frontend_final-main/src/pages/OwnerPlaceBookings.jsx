import { useEffect, useState } from "react";
import { api } from "../services/api";
import { useParams } from "react-router-dom";

export default function OwnerPlaceBookings() {
  const { placeId } = useParams();
  const [bookings, setBookings] = useState([]);

  useEffect(() => {
    (async () => {
      const { data } = await api.get(`/Bookings/place/${placeId}`);
      setBookings(data);
    })();
  }, [placeId]);

  return (
    <div>
      <h2 className="text-xl sm:text-2xl font-bold mb-3">Orders for Template #{placeId}</h2>

      {bookings.length === 0 ? (
        <div className="opacity-70">No orders yet.</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="table">
            <thead>
              <tr>
                <th>Order ID</th>
                <th>Buyer</th>
                <th>Order Date</th>
                <th>Licenses</th>
                <th>Paid</th>
              </tr>
            </thead>
            <tbody>
              {bookings.map(b => (
                <tr key={b.bookingId}>
                  <td>{b.bookingId}</td>
                  <td>{b.user?.name || b.userId}</td>
                  <td>{new Date(b.bookingTime || b.visitDate).toLocaleString()}</td>
                  <td>{b.quantity}</td>
                  <td>{b.paymentConfirmed ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}