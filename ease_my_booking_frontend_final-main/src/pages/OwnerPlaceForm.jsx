import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api } from "../services/api";
import { getPlace, uploadPlaceImages, deletePlaceImage } from "../services/places";
import { Formik, Form, Field } from "formik";
import * as Yup from "yup";
import { notify } from "../utils/toast";

function isGoogleDriveUrl(url) {
  try {
    const u = new URL(url);
    const host = u.hostname.toLowerCase();
    return (
      host === "drive.google.com" ||
      host === "docs.google.com" ||
      host.endsWith(".googleusercontent.com")
    );
  } catch {
    return false;
  }
}

const PlaceSchema = Yup.object({
  name: Yup.string().trim().min(2, "At least 2 characters").required("Name is required"),
  description: Yup.string().trim().min(10, "At least 10 characters").required("Description is required"),
  templateType: Yup.string().trim().required("Template type is required"),
  price: Yup.number()
    .typeError("Price must be a number")
    .min(0, "Price cannot be negative")
    .max(10000000, "Price too large")
    .required("Price is required"),
  imageUrl: Yup.string().trim().url("Must be a valid URL").nullable().optional(),
  googleDriveUrl: Yup.string()
    .trim()
    .url("Enter a valid URL")
    .test("google-drive-only", "Please provide a valid Google Drive / Google Docs link.", (v) => !v || isGoogleDriveUrl(v))
    .optional(),
});

export default function OwnerPlaceForm({ edit = false }) {
  const { placeId } = useParams();
  const navigate = useNavigate();

  const [initialValues, setInitialValues] = useState({
    name: "",
    description: "",
    templateType: "",
    price: 0,
    imageUrl: "",
    googleDriveUrl: "",
  });

  const [loading, setLoading] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState([]);
  const [existingImages, setExistingImages] = useState([]);

  const previews = useMemo(
    () => selectedFiles.map((f) => ({ name: f.name, url: URL.createObjectURL(f) })),
    [selectedFiles]
  );

  useEffect(() => {
    if (edit && placeId) {
      (async () => {
        const p = await getPlace(placeId);
        setInitialValues({
          name: p.name ?? "",
          description: p.description ?? "",
          templateType: p.templateType ?? p.location ?? "",
          price: p.price ?? 0,
          imageUrl: p.imageUrl ?? "",
          googleDriveUrl: p.googleDriveUrl ?? p.googleMapsUrl ?? "",
        });
        setExistingImages((p.images ?? []).sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)));
      })();
    }
    return () => previews.forEach((p) => URL.revokeObjectURL(p.url));
  }, [edit, placeId]); // eslint-disable-line react-hooks/exhaustive-deps

  const onRemoveExisting = async (imgId) => {
    if (!window.confirm("Delete this image?")) return;
    try {
      await deletePlaceImage(placeId, imgId);
      setExistingImages((arr) => arr.filter((i) => i.placeImageId !== imgId));
      notify.success("Image deleted.");
    } catch (e) {
      notify.error(e?.response?.data || "Delete failed", { duration: 3000 });
    }
  };

  return (
    <div className="max-w-3xl mx-auto">
      <h2 className="text-2xl font-bold mb-4">{edit ? "Edit Template" : "Add Template"}</h2>

      <Formik
        initialValues={initialValues}
        validationSchema={PlaceSchema}
        enableReinitialize
        onSubmit={async (values, { setSubmitting }) => {
          try {
            setLoading(true);

            let id = placeId;
            const payload = {
              name: values.name,
              description: values.description,
              templateType: values.templateType,
              price: Number(values.price),
              imageUrl: values.imageUrl?.trim() || null,
              googleDriveUrl: values.googleDriveUrl?.trim() || null,
            };

            if (edit) {
              await notify.promise(api.put(`/Places/${placeId}`, payload), {
                loading: "Saving changes…",
                success: "Template updated.",
                error: (err) => err?.response?.data || "Failed to save",
              });
            } else {
              const { data } = await notify.promise(api.post(`/Places`, payload), {
                loading: "Creating template…",
                success: "Template created.",
                error: (err) => err?.response?.data || "Failed to create",
              });
              id = data.placeId;
            }

            if (selectedFiles.length > 0 && id) {
              await notify.promise(uploadPlaceImages(id, selectedFiles), {
                loading: "Uploading images…",
                success: "Images uploaded.",
                error: (err) => err?.response?.data || "Image upload failed",
              });
            }

            navigate("/dashboard/owner");
          } catch (err) {
            if (err && !err.__handled) {
              notify.error(err?.response?.data || "Failed to save");
            }
          } finally {
            setSubmitting(false);
            setLoading(false);
          }
        }}
      >
        {({ errors, touched, isSubmitting, values }) => (
          <Form className="grid gap-4">
            <div>
              <label className="label"><span className="label-text">Template Name</span></label>
              <Field
                name="name"
                className={`input input-bordered w-full ${touched.name && errors.name ? "input-error" : ""}`}
                placeholder="e.g., MBA Resume Template - Harvard Style"
              />
              {touched.name && errors.name && <div className="text-error text-xs mt-1">{errors.name}</div>}
            </div>

            <div>
              <label className="label"><span className="label-text">Description</span></label>
              <Field
                as="textarea"
                name="description"
                className={`textarea textarea-bordered w-full ${touched.description && errors.description ? "textarea-error" : ""}`}
                minLength={10}
                placeholder="Describe the template, target profile, format, pages, ATS-friendliness, etc."
              />
              {touched.description && errors.description && (
                <div className="text-error text-xs mt-1">{errors.description}</div>
              )}
            </div>

            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <label className="label"><span className="label-text">Template Type</span></label>
                <Field
                  name="templateType"
                  className={`input input-bordered w-full ${touched.templateType && errors.templateType ? "input-error" : ""}`}
                  placeholder="MBA / Consulting / Product Manager / Finance"
                />
                {touched.templateType && errors.templateType && (
                  <div className="text-error text-xs mt-1">{errors.templateType}</div>
                )}
              </div>

              <div>
                <label className="label"><span className="label-text">Price (INR)</span></label>
                <Field
                  name="price"
                  type="number"
                  min="0"
                  step="0.01"
                  className={`input input-bordered w-full ${touched.price && errors.price ? "input-error" : ""}`}
                  placeholder="499"
                />
                {touched.price && errors.price && <div className="text-error text-xs mt-1">{errors.price}</div>}
              </div>
            </div>

            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <label className="label"><span className="label-text">Legacy Cover Image URL (optional)</span></label>
                <Field
                  name="imageUrl"
                  className={`input input-bordered w-full ${touched.imageUrl && errors.imageUrl ? "input-error" : ""}`}
                  placeholder="https://…"
                />
                {touched.imageUrl && errors.imageUrl && (
                  <div className="text-error text-xs mt-1">{errors.imageUrl}</div>
                )}
              </div>

              <div>
                <label className="label"><span className="label-text">Google Drive File Link</span></label>
                <Field
                  name="googleDriveUrl"
                  type="url"
                  className={`input input-bordered w-full ${touched.googleDriveUrl && errors.googleDriveUrl ? "input-error" : ""}`}
                  placeholder="https://drive.google.com/file/d/.../view"
                />
                {touched.googleDriveUrl && errors.googleDriveUrl && (
                  <div className="text-error text-xs mt-1">{errors.googleDriveUrl}</div>
                )}
                {values.googleDriveUrl && !errors.googleDriveUrl && (
                  <a
                    href={values.googleDriveUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="link link-primary mt-1 inline-block"
                  >
                    Preview Drive Link
                  </a>
                )}
              </div>
            </div>

            <div>
              <label className="label"><span className="label-text">Upload Template Images</span></label>
              <input
                type="file"
                multiple
                accept="image/*"
                className="file-input file-input-bordered w-full"
                onChange={(e) => setSelectedFiles(Array.from(e.target.files || []))}
              />
              {selectedFiles.length > 0 && (
                <div className="mt-3 grid grid-cols-2 md:grid-cols-4 gap-3">
                  {previews.map((p) => (
                    <div key={p.url} className="rounded overflow-hidden border">
                      <img src={p.url} alt={p.name} className="w-full aspect-video object-cover" />
                    </div>
                  ))}
                </div>
              )}
            </div>

            {edit && existingImages.length > 0 && (
              <div>
                <div className="label"><span className="label-text">Existing Images</span></div>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  {existingImages.map((img) => (
                    <div key={img.placeImageId} className="relative rounded overflow-hidden border">
                      <img src={img.url} alt="" className="w-full aspect-video object-cover" />
                      <button
                        type="button"
                        onClick={() => onRemoveExisting(img.placeImageId)}
                        className="btn btn-xs btn-error text-white absolute right-2 top-2"
                      >
                        Delete
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <button
              className={`btn btn-primary ${loading || isSubmitting ? "loading" : ""}`}
              disabled={loading || isSubmitting}
              type="submit"
            >
              {edit ? "Save Changes" : "Create Template"}
            </button>
          </Form>
        )}
      </Formik>
    </div>
  );
}