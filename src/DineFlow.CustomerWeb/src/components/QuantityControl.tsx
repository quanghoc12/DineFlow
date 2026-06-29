export function QuantityControl({
  value,
  min,
  onChange
}: {
  value: number;
  min: number;
  onChange: (value: number) => void;
}) {
  return (
    <div className="quantity-control">
      <button onClick={() => onChange(Math.max(min, value - 1))}>-</button>
      <span>{value}</span>
      <button onClick={() => onChange(value + 1)}>+</button>
    </div>
  );
}
