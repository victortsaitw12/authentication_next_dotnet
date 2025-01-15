import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { AuthActions } from "@/app/auth/utils";
import { useSearchParams, useRouter } from "next/navigation";

type FormData = {
  password: string;
};

const ResetPasswordConfirmation = () => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>();
  const router = useRouter();
  const { resetPasswordConfirm } = AuthActions();
  const searchParams = useSearchParams();
  const [uid, setUid] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    if (searchParams.get("uid") && searchParams.get("token")) {
      const uid = searchParams.get("uid");
      const token = searchParams.get("token");
      setUid(uid);
      setToken(token);
    }
  }, [searchParams]);

  const onSubmit = async (data: FormData) => {
    if (!uid || !token) return;
    try {
      await resetPasswordConfirm(
        data.password,
        data.password,
        token,
        uid
      ).res();
      alert("Password reset successful.");
      router.push("/");
    } catch (error) {
      alert("Failed to reset password. Please try again.");
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="px-8 py-6 mt-4 text-left bg-white shadow-lg w-1/3">
        <h3 className="text-2xl font-semibold">Set New Password</h3>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-4">
          <label className="block" htmlFor="password">
            New Password
          </label>
          <input
            type="password"
            placeholder="Enter your new password"
            {...register("password", { required: true })}
            className="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600"
          />
          {errors.password && (
            <span className="text-xs text-red-600">Password is required</span>
          )}
          <div className="flex items-center justify-between mt-4">
            <button className="px-12 py-2 leading-5 text-white transition-colors duration-200 transform bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:bg-blue-700">
              Reset Password
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ResetPasswordConfirmation;
