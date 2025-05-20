import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlockCardComponent } from './block-card.component';

describe('BlockCardComponent', () => {
  let component: BlockCardComponent;
  let fixture: ComponentFixture<BlockCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BlockCardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BlockCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
