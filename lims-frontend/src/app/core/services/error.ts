import { Injectable } from '@angular/core';

export interface ErrorDetails {
    code: number;
    message: string;
    description: string;
    type: 'server' | 'auth' | 'not-found' | 'forbidden' | 'maintenance' | 'network' | 'unknown';
}

@Injectable({
    providedIn: 'root'
})
export class ErrorService {
    private currentError: ErrorDetails | null = null;

    setError(error: ErrorDetails): void {
        this.currentError = error;
    }

    getError(): ErrorDetails | null {
        return this.currentError;
    }

    clearError(): void {
        this.currentError = null;
    }

    mapStatusCodeToType(status: number): ErrorDetails['type'] {
        switch (status) {
            case 0: return 'network';
            case 401: return 'auth';
            case 403: return 'forbidden';
            case 404: return 'not-found';
            case 500: return 'server';
            case 503: return 'maintenance';
            default: return 'unknown';
        }
    }
}
