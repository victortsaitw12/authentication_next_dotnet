import wretch, { Wretch, WretchError } from "wretch";
import { AuthActions } from "@/app/auth/utils";

// Extract necessary functions from the AuthActions utility.
const { handleJWTRefresh, storeToken, getToken } = AuthActions();

const api = () => {
  return (
    wretch("http://localhost:8000")
      // Initialize authentication with the access token.
      .auth(`Bearer ${getToken("access")}`)
      // Catch 401 errors to refresh the token and retry the request.
      .catcher(401, async (error: WretchError, request: Wretch) => {
        try {
          const response = await handleJWTRefresh().json();
          console.log(response);
          // Attempt to refresh the JWT token.
          const { accessToken } = response as {
            accessToken: string;
            refreshToken: string;
          };

          // Store the new access token.
          storeToken(accessToken, "access");

          // Replay the original request with the new access token.
          return request
            .auth(`Bearer ${accessToken}`)
            .fetch()
            .unauthorized(() => {
              console.log("Unauthorized");
              window.location.replace("/");
            })
            .json();
        } catch (err) {
          console.log("Error refreshing token");
          window.location.replace("/");
        }
      })
  );
};

export const fetcher = (url: string): Promise<any> => {
  return api().get(url).json();
};
