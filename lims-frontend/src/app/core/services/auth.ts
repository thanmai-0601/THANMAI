import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api';
import {
  LoginDto, RegisterDto, AuthResponseDto,
  ChangePasswordDto, ResetPasswordDto
} from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<AuthResponseDto | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private api: ApiService, private router: Router) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const stored = localStorage.getItem('NexaLife_user');
    if (stored) {
      const user: AuthResponseDto = JSON.parse(stored);
      // Check if token is expired
      if (new Date(user.expiresAt) > new Date()) {
        this.currentUserSubject.next(user);
      } else {
        this.logout();
      }
    }
  }

  login(dto: LoginDto): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('auth/login', dto).pipe(
      tap(response => {
        localStorage.setItem('NexaLife_user', JSON.stringify(response));
        localStorage.setItem('NexaLife_token', response.token);
        this.currentUserSubject.next(response);
      })
    );
  }

  register(dto: RegisterDto): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('auth/register', dto);
  }

  changePassword(dto: ChangePasswordDto): Observable<any> {
    return this.api.post('auth/change-password', dto);
  }

  resetPassword(dto: ResetPasswordDto): Observable<any> {
    return this.api.post('auth/reset-password', dto);
  }

  getCurrentUserFromApi(): Observable<any> {
    return this.api.get('auth/me');
  }

  logout(): void {
    localStorage.removeItem('NexaLife_user');
    localStorage.removeItem('NexaLife_token');
    this.currentUserSubject.next(null);
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return localStorage.getItem('NexaLife_token');
  }

  isLoggedIn(): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    return new Date(user.expiresAt) > new Date();
  }

  getUserRole(): string | null {
    return this.currentUserSubject.value?.role || null;
  }

  getUserName(): string | null {
    return this.currentUserSubject.value?.fullName || null;
  }

  getUserEmail(): string | null {
    return this.currentUserSubject.value?.email || null;
  }

  getUserId(): number | null {
    return this.currentUserSubject.value?.userId || null;
  }


  getDashboardRoute(): string {
    const role = this.getUserRole();
    switch (role) {
      case 'Admin': return '/app/dashboard/admin';
      case 'Agent': return '/app/dashboard/agent';
      case 'Customer': return '/app/dashboard/customer';
      case 'ClaimsOfficer': return '/app/dashboard/claims-officer';
      default: return '/login';
    }
  }
}
