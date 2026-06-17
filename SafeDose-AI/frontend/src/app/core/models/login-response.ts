export interface LoginResponse {
  message: string;
  isAuthenticated: boolean;
  userName: string;
  email: string;
  expiresOn: string;
  token: string;
}
