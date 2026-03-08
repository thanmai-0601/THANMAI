import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ErrorService, ErrorDetails } from '../../../core/services/error';

@Component({
    selector: 'app-global-error',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './global-error.html'
})
export class GlobalError implements OnInit {
    error: ErrorDetails | null = null;
    showDetails = false;

    constructor(
        private router: Router,
        private errorService: ErrorService
    ) { }

    ngOnInit(): void {
        this.error = this.errorService.getError();
        // If no error found, could be a direct navigation, set as unknown
        if (!this.error) {
            this.error = {
                code: 404,
                message: 'Unknown Error',
                description: 'The page you are looking for might have moved or is temporarily unavailable.',
                type: 'not-found'
            };
        }
    }

    refresh(): void {
        this.errorService.clearError();
        window.location.reload();
    }

    toggleDetails(): void {
        this.showDetails = !this.showDetails;
    }

    goHome(): void {
        this.errorService.clearError();
        const role = localStorage.getItem('NexaLife_role');
        if (role === 'Admin') this.router.navigate(['/app/dashboard/admin']);
        else if (role === 'Agent') this.router.navigate(['/app/dashboard/agent']);
        else if (role === 'Customer') this.router.navigate(['/app/dashboard/customer']);
        else if (role === 'ClaimsOfficer') this.router.navigate(['/app/dashboard/claims-officer']);
        else this.router.navigate(['/']);
    }
}
