import wretch, { Wretch, WretchError } from "wretch";
import { AuthActions } from "./auth/utils";
const { handleJWTRefresh, storeToken, getToken } = AuthActions();

const api = () => {
  return (
    wretch("http://localhost:8000")
      // Initialize authentication with the access token
      .auth(`Bearer ${getToken("access")}`)
      // Add CORS headers
      .headers({
        "Content-Type": "application/json",
        Accept: "application/json",
      })
      // Catch 401 errors and refresh the access token and retry the request
      .catcher(401, async (error, request) => {
        try {
          // Attempt to refresh the JWT token.
          const response = await handleJWTRefresh().json();
          const { accessToken } = response as { accessToken: string };
          // Store the new access token
          storeToken(accessToken, "access");
          // Retry the request with the new access token
          return request
            .auth(`Bearer ${accessToken}`)
            .fetch()
            .unauthorized(() => {
              window.location.replace("/");
            })
            .json();
        } catch (error) {
          window.location.replace("/");
        }
      })
  );
};

export const fetcher = (url: string): Promise<any> => {
  return api().get(url).json();
};
