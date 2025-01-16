import wretch from "wretch";
import Cookies from "js-cookie";
// Base API setup for making requests to the backend
const api = wretch("http://localhost:8000/api").accept("application/json");

const storeToken = (token: string, type: "access" | "refresh") => {
  Cookies.set(type + "Token", token);
};

const getToken = (type: "access" | "refresh") => {
  return Cookies.get(type + "Token");
};

const removeTokens = () => {
  Cookies.remove("accessToken");
  Cookies.remove("refreshToken");
};

const register = (email: string, username: string, password: string) => {
  return api.post({ email, username, password }, "/auth/users/");
};

const login = (email: string, password: string) => {
  return api.post({ email, password }, "/auth/jwt/create");
};

const logout = () => {
  const refreshToken = getToken("refresh");
  return api.post({ refreshToken: refreshToken }, "/auth/logout/");
};

const handleJWTRefresh = () => {
  const refreshToken = getToken("refresh");
  return api.post({ refreshToken: refreshToken }, "/auth/jwt/refresh");
};

const resetPassword = (email: string) => {
  return api.post({ email }, "/auth/users/reset_password/");
};

const resetPasswordConfirm = (
  new_password: string,
  re_new_password: string,
  uid: string,
  token: string
) => {
  return api.post(
    { uid, token, new_password, re_new_password },
    "/auth/users/reset_password_confirm/"
  );
};

export const AuthActions = () => {
  return {
    register,
    login,
    logout,
    handleJWTRefresh,
    resetPassword,
    resetPasswordConfirm,
    storeToken,
    getToken,
    removeTokens,
  };
};
