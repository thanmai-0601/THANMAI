export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
  dateOfBirth: string;
  bankAccountName?: string;
  bankAccountNumber?: string;
  bankIfscCode?: string;
}

export interface AuthResponseDto {
  token: string;
  fullName: string;
  email: string;
  role: string;
  userId: number;
  expiresAt: string;
  mustChangePassword: boolean;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ResetPasswordDto {
  email: string;
  newPassword: string;
}

export interface CreateStaffDto {
  fullName: string;
  email: string;
  phoneNumber: string;
  password?: string;
  dateOfBirth: string;
  bankAccountName?: string;
  bankAccountNumber?: string;
  bankIfscCode?: string;
}

export interface UpdateStaffDto {
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
}

export interface UserListDto {
  userId: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface StaffResponseDto {
  userId: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  temporaryPassword: string;
}
