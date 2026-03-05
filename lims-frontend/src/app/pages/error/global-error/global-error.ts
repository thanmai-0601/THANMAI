import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
    selector: 'app-global-error',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './global-error.html'
})
export class GlobalError {

    constructor(private router: Router) { }

    refresh(): void {
        window.location.reload();
    }

    goHome(): void {
        const role = localStorage.getItem('NexaLife_role');
        if (role === 'Admin') this.router.navigate(['/app/dashboard/admin']);
        else if (role === 'Agent') this.router.navigate(['/app/dashboard/agent']);
        else if (role === 'Customer') this.router.navigate(['/app/dashboard/customer']);
        else if (role === 'ClaimsOfficer') this.router.navigate(['/app/dashboard/claims-officer']);
        else this.router.navigate(['/']);
    }
}
