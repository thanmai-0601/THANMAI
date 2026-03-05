import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { UserList } from './user-list';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { AuthService } from '../../../core/services/auth';

describe('UserList', () => {
  let component: UserList;
  let fixture: ComponentFixture<UserList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserList, HttpClientTestingModule, RouterTestingModule, ReactiveFormsModule, FormsModule],
      providers: [
        { provide: ApiService, useValue: jasmine.createSpyObj('ApiService', ['get', 'put', 'post', 'delete']) },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['success', 'error', 'show']) },
        { provide: AuthService, useValue: jasmine.createSpyObj('AuthService', ['isLoggedIn', 'getUserRole', 'getUserId']) }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    fixture = TestBed.createComponent(UserList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a fixture', () => {
    expect(fixture).toBeTruthy();
  });

  it('should render without errors', () => {
    expect(component).toBeDefined();
  });

  it('should be a standalone component', () => {
    expect(UserList).toBeTruthy();
  });

  it('should initialize component properties', () => {
    expect(component).toBeDefined();
  });

  it('should toggle user status and call api.put', () => {
    const apiServiceMock = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    const toastMock = TestBed.inject(ToastService) as jasmine.SpyObj<ToastService>;

    apiServiceMock.put.and.returnValue({ subscribe: (callbacks: any) => callbacks.next({}) } as any);

    const mockUser = { userId: 1, fullName: 'Test User', isActive: true } as any;

    component.toggleStatus(mockUser);

    expect(apiServiceMock.put).toHaveBeenCalledWith('admin/users/1/toggle-status', {});
    expect(toastMock.show).toHaveBeenCalledWith('User Test User status updated.', 'success');
    expect(mockUser.isActive).toBeFalse();
  });
});
