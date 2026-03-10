import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }
    return this.http.get<T>(`${this.baseUrl}/${path}`, { params: httpParams });
  }

  post<T>(path: string, body: any = {}): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${path}`, body);
  }

  put<T>(path: string, body: any = {}): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${path}`, body);
  }

  patch<T>(path: string, body: any = {}): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${path}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${path}`);
  }

  /**
   * Static helper to extract a user-friendly error message from a backend error response.
   * Handles Message, message, validation errors, and plain strings.
   */
  public static getErrorMessage(error: any): string {
    if (!error) return 'An unexpected error occurred.';

    // If it's a direct HttpErrorResponse or similar object containing 'error'
    const errorBody = error.error || error;

    // 1. Check for ASP.NET Validation Errors (ModelState-style)
    if (errorBody?.errors) {
      const firstErrorSet = Object.values(errorBody.errors)[0] as any[];
      if (firstErrorSet && firstErrorSet.length > 0) return firstErrorSet[0];
    }

    // 2. Check for standard 'Message' or 'message' property
    if (errorBody?.Message) return errorBody.Message;
    if (errorBody?.message) return errorBody.message;

    // 3. Fallback to status text if status is available
    if (error.statusText && error.status !== 0) return error.statusText;

    // 4. Ultimate fallback
    return typeof errorBody === 'string' ? errorBody : 'Task failed. Please try again.';
  }
}
