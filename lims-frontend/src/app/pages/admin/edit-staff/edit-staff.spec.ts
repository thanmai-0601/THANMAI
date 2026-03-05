import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditStaff } from './edit-staff';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { UserListDto } from '../../../core/models/auth.model';

describe('EditStaff Component', () => {
    let component: EditStaff;
    let fixture: ComponentFixture<EditStaff>;
    let apiServiceMock: any;
    let toastServiceMock: any;
    let routerMock: any;
    let routeMock: any;

    beforeEach(async () => {
        apiServiceMock = {
            get: jasmine.createSpy('get').and.returnValue(of({
                userId: 1,
                fullName: 'Test Agent',
                email: 'agent@test.com',
                phoneNumber: '1234567890',
                role: 'Agent'
            })),
            put: jasmine.createSpy('put').and.returnValue(of({}))
        };

        toastServiceMock = {
            show: jasmine.createSpy('show')
        };

        routerMock = {
            navigate: jasmine.createSpy('navigate')
        };

        routeMock = {
            snapshot: {
                paramMap: {
                    get: jasmine.createSpy('get').and.returnValue('1')
                }
            }
        };

        await TestBed.configureTestingModule({
            imports: [EditStaff, ReactiveFormsModule],
            providers: [
                { provide: ApiService, useValue: apiServiceMock },
                { provide: ToastService, useValue: toastServiceMock },
                { provide: Router, useValue: routerMock },
                { provide: ActivatedRoute, useValue: routeMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(EditStaff);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create and load user details on init', () => {
        expect(component).toBeTruthy();
        expect(apiServiceMock.get).toHaveBeenCalledWith('admin/users/1');
        expect(component.editStaffForm.value.fullName).toBe('Test Agent');
        expect(component.loading).toBeFalse();
    });

    it('should redirect if user is Admin or Customer', () => {
        apiServiceMock.get.and.returnValue(of({ role: 'Admin' } as UserListDto));

        component.ngOnInit();

        expect(toastServiceMock.show).toHaveBeenCalledWith('Cannot edit Admin accounts from here.', 'error');
        expect(routerMock.navigate).toHaveBeenCalledWith(['/app/admin/users']);
    });

    it('should validate form and not submit if invalid', () => {
        component.editStaffForm.patchValue({ email: 'invalid-email' }); // invalid email
        component.submit();

        expect(toastServiceMock.show).toHaveBeenCalledWith('Please fill in all details.', 'warning');
        expect(apiServiceMock.put).not.toHaveBeenCalled();
    });

    it('should call api.put with correct payload on valid submit', () => {
        component.editStaffForm.patchValue({
            fullName: 'Updated Name',
            email: 'updated@test.com',
            phoneNumber: '9876543210',
            role: 'ClaimsOfficer'
        });

        component.submit();

        expect(component.submitting).toBeTrue();
        expect(apiServiceMock.put).toHaveBeenCalledWith('admin/users/1', {
            fullName: 'Updated Name',
            email: 'updated@test.com',
            phoneNumber: '9876543210',
            role: 'ClaimsOfficer'
        });
        expect(toastServiceMock.show).toHaveBeenCalledWith('Staff details updated successfully!', 'success');
        expect(routerMock.navigate).toHaveBeenCalledWith(['/app/admin/users']);
    });
});
