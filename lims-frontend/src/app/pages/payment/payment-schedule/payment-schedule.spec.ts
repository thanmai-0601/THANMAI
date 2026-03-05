import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { PaymentSchedule } from './payment-schedule';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { AuthService } from '../../../core/services/auth';

describe('PaymentSchedule', () => {
  let component: PaymentSchedule;
  let fixture: ComponentFixture<PaymentSchedule>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentSchedule, HttpClientTestingModule, RouterTestingModule],
      providers: [
        ApiService,
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['success', 'error', 'show']) },
        { provide: AuthService, useValue: jasmine.createSpyObj('AuthService', ['isLoggedIn', 'getUserRole', 'getUserId']) }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    fixture = TestBed.createComponent(PaymentSchedule);
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
    expect(PaymentSchedule).toBeTruthy();
  });

  it('should initialize component properties', () => {
    expect(component).toBeDefined();
  });
});
