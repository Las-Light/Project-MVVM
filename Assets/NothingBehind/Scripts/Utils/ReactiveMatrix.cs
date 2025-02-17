using R3;

namespace NothingBehind.Scripts.Utils
{
    public class ReactiveMatrix<T>
    {
        private readonly T[,] _data;
        private readonly Subject<(int Row, int Col, T Value)> _onChange = new Subject<(int, int, T)>();

        public ReactiveMatrix(int rows, int cols)
        {
            _data = new T[rows, cols];
        }

        public void SetValue(int row, int col, T value)
        {
            if (row >= 0 && row < _data.GetLength(0) && col >= 0 && col < _data.GetLength(1))
            {
                _data[row, col] = value;
                _onChange.OnNext((row, col, value));
            }
        }

        public T GetValue(int row, int col)
        {
            return _data[row, col];
        }

        public T[,] GetMatrix()
        {
            return _data;
        }

        public T[] GetArray()
        {
            var array = new T[_data.GetLength(0) * _data.GetLength(1)];
            // Преобразуем двумерный массив в одномерный
            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    array[i * _data.GetLength(1) + j] = GetValue(i, j);
                }
            }

            return array;
        }

        public Observable<(int Row, int Col, T Value)> OnChange => _onChange.AsObservable();
    }
}