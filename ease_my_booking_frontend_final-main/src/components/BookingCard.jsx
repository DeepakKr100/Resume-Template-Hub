import { formatCurrency } from "../utils/format";
import DriveLink from "./DriveLink";

export default function BookingCard({ booking, onPayNow, onWriteReview }) {
  const displayDate = new Date(booking.bookingTime || booking.visitDate);
  const total = (booking.place?.price ?? 0) * booking.quantity;

  const thumb =
    booking.placeThumbUrl ||
    booking.place?.thumbnailUrl ||
    booking.place?.imageUrl ||
    "";

  const templateType = booking.place?.templateType || booking.place?.location || "N/A";

  return (
    <div className="card bg-base-100 shadow-sm">
      <div className="card-body">
        <div className="flex items-stretch gap-4">
          <div className="flex-1 min-w-0">
            <h4 className="card-title truncate">
              {booking.place?.name ?? "Template"}
            </h4>

            <div className="flex flex-row justify-between gap-3">
              <div>
                <div className="text-sm opacity-70">
                  Order: {displayDate.toLocaleString()}
                </div>
                <div className="text-sm">Licenses: {booking.quantity}</div>
                <div className="text-sm">Type: {templateType}</div>
                <div className="text-sm">Total: {formatCurrency(total)}</div>

                <div className="mt-2 flex items-center gap-3 flex-wrap">
                  {booking.paymentConfirmed ? (
                    <>
                      <span className="badge badge-success">Paid</span>
                      {booking.deliveryUrl ? (
                        <DriveLink
                          url={booking.deliveryUrl}
                          label="Access File"
                          size="sm"
                          variant="outline"
                        />
                      ) : (
                        <span className="badge badge-warning">
                          File link not added by seller yet
                        </span>
                      )}
                    </>
                  ) : (
                    <>
                      <span className="badge badge-error">Payment Pending</span>
                      {onPayNow && (
                        <button
                          className="btn btn-xs btn-primary"
                          onClick={() => onPayNow(booking)}
                          type="button"
                        >
                          Pay Now
                        </button>
                      )}
                    </>
                  )}

                  {onWriteReview && (
                    <button
                      className="btn btn-link btn-xs px-0"
                      onClick={() =>
                        onWriteReview({
                          placeId: booking.place?.placeId ?? booking.placeId,
                          placeName: booking.place?.name ?? "Template",
                        })
                      }
                      type="button"
                    >
                      Write Review
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>

          <div className="w-40 h-28 md:w-64 md:h-40 rounded-lg overflow-hidden bg-base-300 shrink-0">
            {thumb ? (
              <img
                src={thumb}
                alt={booking.place?.name ?? "Template"}
                className="w-full h-full object-cover"
                loading="lazy"
              />
            ) : (
              <div className="w-full h-full grid place-items-center text-xs opacity-60">
                No Image
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}