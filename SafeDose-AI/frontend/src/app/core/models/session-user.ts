export type UserRole = 'User' | 'Admin' | 'SuperAdmin';

export interface SessionUser {
  userName: string;
  email: string;
  role: UserRole;
}
